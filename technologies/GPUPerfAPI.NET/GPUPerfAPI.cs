using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using gpa_int8 = System.SByte;
using gpa_int16 = System.Int16;
using gpa_int32 = System.Int32;
using gpa_int64 = System.Int64;
using gpa_float32 = System.Single;
using gpa_float64 = System.Double;
using gpa_uint8 = System.Byte;
using gpa_uint16 = System.UInt16;
using gpa_uint32 = System.UInt32;
using gpa_uint64 = System.UInt64;
//using GPA_LoggingCallbackPtrType = System.IntPtr;

namespace GPUPerfAPI
{
    /// Status enumerations
    public enum GPA_Status
    {
       GPA_STATUS_OK = 0,
       GPA_STATUS_ERROR_NULL_POINTER,
       GPA_STATUS_ERROR_COUNTERS_NOT_OPEN,
       GPA_STATUS_ERROR_COUNTERS_ALREADY_OPEN,
       GPA_STATUS_ERROR_INDEX_OUT_OF_RANGE,
       GPA_STATUS_ERROR_NOT_FOUND,
       GPA_STATUS_ERROR_ALREADY_ENABLED,
       GPA_STATUS_ERROR_NO_COUNTERS_ENABLED,
       GPA_STATUS_ERROR_NOT_ENABLED,
       GPA_STATUS_ERROR_SAMPLING_NOT_STARTED,
       GPA_STATUS_ERROR_SAMPLING_ALREADY_STARTED,
       GPA_STATUS_ERROR_SAMPLING_NOT_ENDED,
       GPA_STATUS_ERROR_NOT_ENOUGH_PASSES,
       GPA_STATUS_ERROR_PASS_NOT_ENDED,
       GPA_STATUS_ERROR_PASS_NOT_STARTED,
       GPA_STATUS_ERROR_PASS_ALREADY_STARTED,
       GPA_STATUS_ERROR_SAMPLE_NOT_STARTED,
       GPA_STATUS_ERROR_SAMPLE_ALREADY_STARTED,
       GPA_STATUS_ERROR_SAMPLE_NOT_ENDED,
       GPA_STATUS_ERROR_CANNOT_CHANGE_COUNTERS_WHEN_SAMPLING,
       GPA_STATUS_ERROR_SESSION_NOT_FOUND,
       GPA_STATUS_ERROR_SAMPLE_NOT_FOUND,
       GPA_STATUS_ERROR_SAMPLE_NOT_FOUND_IN_ALL_PASSES,
       GPA_STATUS_ERROR_COUNTER_NOT_OF_SPECIFIED_TYPE,
       GPA_STATUS_ERROR_READING_COUNTER_RESULT,
       GPA_STATUS_ERROR_VARIABLE_NUMBER_OF_SAMPLES_IN_PASSES,
       GPA_STATUS_ERROR_FAILED,
       GPA_STATUS_ERROR_HARDWARE_NOT_SUPPORTED,
    };

    /// Value type definitions
    public enum GPA_Type
    {
       GPA_TYPE_FLOAT32,             ///< Result will be a 32-bit float
       GPA_TYPE_FLOAT64,             ///< Result will be a 64-bit float
       GPA_TYPE_UINT32,              ///< Result will be a 32-bit unsigned int
       GPA_TYPE_UINT64,              ///< Result will be a 64-bit unsigned int
       GPA_TYPE_INT32,               ///< Result will be a 32-bit int
       GPA_TYPE_INT64,               ///< Result will be a 64-bit int
       GPA_TYPE__LAST                ///< Marker indicating last element
    };

    /// Result usage type definitions
    public enum GPA_Usage_Type
    {
       GPA_USAGE_TYPE_RATIO,         ///< Result is a ratio of two different values or types
       GPA_USAGE_TYPE_PERCENTAGE,    ///< Result is a percentage, typically within [0,100] range, but may be higher for certain counters
       GPA_USAGE_TYPE_CYCLES,        ///< Result is in clock cycles
       GPA_USAGE_TYPE_MILLISECONDS,  ///< Result is in milliseconds
       GPA_USAGE_TYPE_BYTES,         ///< Result is in bytes
       GPA_USAGE_TYPE_ITEMS,         ///< Result is a count of items or objects (ie, vertices, triangles, threads, pixels, texels, etc)
       GPA_USAGE_TYPE_KILOBYTES,     ///< Result is in kilobytes
       GPA_USAGE_TYPE__LAST          ///< Marker indicating last element
    };

    /// Logging type definitions
    public enum GPA_Logging_Type
    {
       GPA_LOGGING_NONE = 0,
       GPA_LOGGING_ERROR = 1,
       GPA_LOGGING_MESSAGE = 2,
       GPA_LOGGING_ERROR_AND_MESSAGE = 3,
       GPA_LOGGING_TRACE = 4,
       GPA_LOGGING_ERROR_AND_TRACE = 5,
       GPA_LOGGING_MESSAGE_AND_TRACE = 6,
       GPA_LOGGING_ERROR_MESSAGE_AND_TRACE = 7,
       GPA_LOGGING_ALL = 0xFF
    };

    /// typedef void( *GPA_LoggingCallbackPtrType ) ( GPA_Logging_Type messageType, const char* message );

    //  http://www.gamedev.net/topic/270670-pinvoke-callback-access-violation/
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GPA_LoggingCallbackDelegate(GPA_Logging_Type messageType, string message);

    public class GPA
    {
        /// \brief Register a callback function to receive log messages.
        ///
        /// Only one callback function can be registered, so the implementation should be able
        /// to handle the different types of messages. A parameter to the callback function will
        /// indicate the message type being received. Messages will not contain a newline character
        /// at the end of the message.
        /// \param loggingType Identifies the type of messages to receive callbacks for.
        /// \param callbackFuncPtr Pointer to the callback function
        /// \return GPA_STATUS_OK, unless the callbackFuncPtr is NULL and the loggingType is not 
        /// GPA_LOGGING_NONE, in which case GPA_STATUS_ERROR_NULL_POINTER is returned.
        /// GPALIB_DECL GPA_Status GPA_RegisterLoggingCallback( GPA_Logging_Type loggingType, GPA_LoggingCallbackPtrType callbackFuncPtr );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_RegisterLoggingCallback(GPA_Logging_Type loggingType, GPA_LoggingCallbackDelegate callbackFuncPtr);

        // Init / destroy GPA

        /// \brief Initializes the driver so that counters are exposed.
        ///
        /// This function must be called before the rendering context or device is created.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_Initialize( );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_Initialize();

        /// \brief Undo any initialization to ensure proper behavior in applications that are not being profiled.
        ///
        /// This function must be called after the rendering context or device is released / destroyed.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_Destroy( );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_Destroy();

        // Context Startup / Finish

        /// \brief Opens the counters in the specified context for reading.
        ///
        /// This function must be called before any other GPA functions.
        /// \param context The context to open counters for. Typically a device pointer. Refer to GPA API specific documentation for further details.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_OpenContext( void* context );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_OpenContext(IntPtr context);

        /// \brief Closes the counters in the currently active context.
        ///
        /// GPA functions should not be called again until the counters are reopened with GPA_OpenContext.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_CloseContext();
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_CloseContext();

        /// \brief Select another context to be the currently active context.
        ///
        /// The selected context must have previously been opened with a call to GPA_OpenContext.
        /// If the call is successful, all GPA functions will act on the currently selected context.
        /// \param context The context to select. The same value that was passed to GPA_OpenContext.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_SelectContext( void* context );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_SelectContext(IntPtr context);

        // Counter Interrogation

        /// \brief Get the number of counters available.
        ///
        /// \param count The value which will hold the count upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetNumCounters( gpa_uint32* count );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetNumCounters(out gpa_uint32 count);

        /// \brief Get the name of a specific counter.
        ///
        /// \param index The index of the counter name to query. Must lie between 0 and (GPA_GetNumCounters result - 1).
        /// \param name The value which will hold the name upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetCounterName( gpa_uint32 index, const char** name );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetCounterName( gpa_uint32 index, out string name);

        /// \brief Get description of the specified counter.
        ///
        /// \param index The index of the counter to query. Must lie between 0 and (GPA_GetNumCounters result - 1).
        /// \param description The value which will hold the description upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetCounterDescription( gpa_uint32 index, const char** description );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetCounterDescription(gpa_uint32 index, out string description);

        /// \brief Get the counter data type of the specified counter.
        ///
        /// \param index The index of the counter. Must lie between 0 and (GPA_GetNumCounters result - 1).
        /// \param counterDataType The value which will hold the description upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetCounterDataType( gpa_uint32 index, GPA_Type* counterDataType );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetCounterDataType(gpa_uint32 index, out GPA_Type counterDataType);

        /// \brief Get the counter usage type of the specified counter.
        ///
        /// \param index The index of the counter. Must lie between 0 and (GPA_GetNumCounters result - 1).
        /// \param counterUsageType The value which will hold the description upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetCounterUsageType( gpa_uint32 index, GPA_Usage_Type* counterUsageType );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetCounterUsageType(gpa_uint32 index, out GPA_Usage_Type counterUsageType);

        /// \brief Get a string with the name of the specified counter data type.
        ///
        /// Typically used to display counter types along with their name.
        /// E.g. counterDataType of GPA_TYPE_UINT64 would return "gpa_uint64".
        /// \param counterDataType The type to get the string for.
        /// \param typeStr The value that will be set to contain a reference to the name of the counter data type.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetDataTypeAsStr( GPA_Type counterDataType, const char** typeStr );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetDataTypeAsStr(GPA_Type counterDataType, out string typeStr);

        /// \brief Get a string with the name of the specified counter usage type.
        ///
        /// Convertes the counter usage type to a string representation.
        /// E.g. counterUsageType of GPA_USAGE_TYPE_PERCENTAGE would return "percentage".
        /// \param counterUsageType The type to get the string for.
        /// \param usageTypeStr The value that will be set to contain a reference to the name of the counter usage type.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetUsageTypeAsStr( GPA_Usage_Type counterUsageType, const char** usageTypeStr );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetUsageTypeAsStr(GPA_Usage_Type counterUsageType, out string usageTypeStr);

        /// \brief Enable a specified counter.
        ///
        /// Subsequent sampling sessions will provide values for any enabled counters.
        /// Initially all counters are disabled, and must explicitly be enabled by calling this function.
        /// \param index The index of the counter to enable. Must lie between 0 and (GPA_GetNumCounters result - 1).
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_EnableCounter( gpa_uint32 index );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_EnableCounter(gpa_uint32 index);

        /// \brief Disable a specified counter.
        ///
        /// Subsequent sampling sessions will not provide values for any disabled counters.
        /// Initially all counters are disabled, and must explicitly be enabled.
        /// \param index The index of the counter to enable. Must lie between 0 and (GPA_GetNumCounters result - 1).
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_DisableCounter( gpa_uint32 index );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_DisableCounter(gpa_uint32 index);

        /// \brief Get the number of enabled counters.
        ///
        /// \param count The value that will be set to the number of counters that are currently enabled.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetEnabledCount( gpa_uint32* count );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetEnabledCount(out gpa_uint32 count);

        /// \brief Get the counter index for an enabled counter.
        ///
        /// For example, if GPA_GetEnabledIndex returns 3, and I wanted the counter index of the first enabled counter,
        /// I would call this function with enabledNumber equal to 0.
        /// \param enabledNumber The number of the enabled counter to get the counter index for. Must lie between 0 and (GPA_GetEnabledIndex result - 1).
        /// \param enabledCounterIndex The value that will contain the index of the counter.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetEnabledIndex( gpa_uint32 enabledNumber, gpa_uint32* enabledCounterIndex );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetEnabledIndex(gpa_uint32 enabledNumber, out gpa_uint32 enabledCounterIndex);

        /// \brief Check that a counter is enabled.
        ///
        /// \param counterIndex The index of the counter. Must lie between 0 and (GPA_GetNumCounters result - 1).
        /// \return GPA_STATUS_OK is returned if the counter is enabled, GPA_STATUS_ERROR_NOT_FOUND otherwise.
        /// GPALIB_DECL GPA_Status GPA_IsCounterEnabled( gpa_uint32 counterIndex );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_IsCounterEnabled(gpa_uint32 counterIndex);

        /// \brief Enable a specified counter using the counter name (case insensitive).
        ///
        /// Subsequent sampling sessions will provide values for any enabled counters.
        /// Initially all counters are disabled, and must explicitly be enabled by calling this function.
        /// \param counter The name of the counter to enable.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_EnableCounterStr( const char* counter );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_EnableCounterStr(string counter);

        /// \brief Disable a specified counter using the counter name (case insensitive).
        ///
        /// Subsequent sampling sessions will not provide values for any disabled counters.
        /// Initially all counters are disabled, and must explicitly be enabled.
        /// \param counter The name of the counter to disable.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_DisableCounterStr( const char* counter );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_DisableCounterStr(string counter);

        /// \brief Enable all counters.
        ///
        /// Subsequent sampling sessions will provide values for all counters.
        /// Initially all counters are disabled, and must explicitly be enabled by calling a function which enables them.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_EnableAllCounters();
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_EnableAllCounters();

        /// \brief Disable all counters.
        ///
        /// Subsequent sampling sessions will not provide values for any disabled counters.
        /// Initially all counters are disabled, and must explicitly be enabled.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_DisableAllCounters();
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_DisableAllCounters();

        /// \brief Get index of a counter given its name (case insensitive).
        ///
        /// \param counter The name of the counter to get the index for.
        /// \param index The index of the requested counter.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetCounterIndex( const char* counter, gpa_uint32* index );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetCounterIndex(string counter, out gpa_uint32 index);

        /// \brief Get the number of passes required for the currently enabled set of counters.
        ///
        /// This represents the number of times the same sequence must be repeated to capture the counter data.
        /// On each pass a different (compatible) set of counters will be measured.
        /// \param numPasses The value of the number of passes.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetPassCount( gpa_uint32* numPasses );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetPassCount(out gpa_uint32 numPasses);

        /// \brief Begin sampling with the currently enabled set of counters.
        ///
        /// This must be called to begin the counter sampling process.
        /// A unique sessionID will be returned which is later used to retrieve the counter values.
        /// Session Identifiers are integers and always start from 1 on a newly opened context, upwards in sequence.
        /// The set of enabled counters cannot be changed inside a BeginSession/EndSession sequence.
        /// \param sessionID The value that will be set to the session identifier.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_BeginSession( gpa_uint32* sessionID );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_BeginSession(out gpa_uint32 sessionID);

        /// \brief End sampling with the currently enabled set of counters.
        ///
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_EndSession();
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_EndSession();

        /// \brief Begin sampling pass.
        ///
        /// Between BeginPass and EndPass calls it is expected that a sequence of repeatable operations exist.
        /// If this is not the case only counters which execute in a single pass should be activated.
        /// The number of required passes can be determined by enabling a set of counters and then calling GPA_GetPassCount.
        /// The operations inside the BeginPass/EndPass calls should be looped over GPA_GetPassCount result number of times.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_BeginPass();
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_BeginPass();

        /// \brief End sampling pass.
        ///
        /// Between BeginPass and EndPass calls it is expected that a sequence of repeatable operations exist.
        /// If this is not the case only counters which execute in a single pass should be activated.
        /// The number of required passes can be determined by enabling a set of counters and then calling GPA_GetPassCount.
        /// The operations inside the BeginPass/EndPass calls should be looped over GPA_GetPassCount result number of times.
        /// This is necessary to capture all counter values, since sometimes counter combinations cannot be captured simultaneously.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_EndPass();
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_EndPass();

        /// \brief Begin a sample using the enabled counters.
        ///
        /// Multiple samples can be performed inside a BeginSession/EndSession sequence.
        /// Each sample computes the values of the counters between BeginSample and EndSample.
        /// To identify each sample the user must provide a unique sampleID as a paramter to this function.
        /// The number need only be unique within the same BeginSession/EndSession sequence.
        /// BeginSample must be followed by a call to EndSample before BeginSample is called again.
        /// \param sampleID Any integer, unique within the BeginSession/EndSession sequence, used to retrieve the sample results.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_BeginSample( gpa_uint32 sampleID );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_BeginSample(gpa_uint32 sampleID);

        /// \brief End sampling using the enabled counters.
        ///
        /// BeginSample must be followed by a call to EndSample before BeginSample is called again.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_EndSample();
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_EndSample();

        /// \brief Get the number of samples a specified session contains.
        ///
        /// This is useful if samples are conditionally created and a count is not kept.
        /// \param sessionID The session to get the number of samples for.
        /// \param samples The value that will be set to the number of samples contained within the session.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetSampleCount( gpa_uint32 sessionID, gpa_uint32 *samples );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetSampleCount(gpa_uint32 sessionID, out gpa_uint32 samples);

        /// \brief Determine if an individual sample result is available.
        ///
        /// After a sampling session results may be available immediately or take a certain amount of time to become available.
        /// This function allows you to determine when a sample can be read. 
        /// The function does not block, permitting periodic polling.
        /// To block until a sample is ready use a GetSample* function instead of this.
        /// It can be more efficient to determine if a whole session's worth of data is available using GPA_IsSessionReady.
        /// \param readyResult The value that will contain the result of the sample being ready. True if ready.
        /// \param sessionID The session containing the sample to determine availability.
        /// \param sampleID The sample identifier of the sample to query availability for.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_IsSampleReady( bool* readyResult, gpa_uint32 sessionID, gpa_uint32 sampleID );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_IsSampleReady(out bool readyResult, gpa_uint32 sessionID, gpa_uint32 sampleID);

        /// \brief Determine if all samples within a session are available.
        ///
        /// After a sampling session results may be available immediately or take a certain amount of time to become available.
        /// This function allows you to determine when the results of a session can be read. 
        /// The function does not block, permitting periodic polling.
        /// To block until a sample is ready use a GetSample* function instead of this.
        /// \param readyResult The value that will contain the result of the session being ready. True if ready.
        /// \param sessionID The session to determine availability for.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_IsSessionReady( bool* readyResult, gpa_uint32 sessionID );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_IsSessionReady(out bool readyResult, gpa_uint32 sessionID);

        /// \brief Get a sample of type 64-bit unsigned integer.
        ///
        /// This function will block until the value is available.
        /// Use GPA_IsSampleReady if you do not wish to block.
        /// \param sessionID The session identifier with the sample you wish to retreive the result of.
        /// \param sampleID The identifier of the sample to get the result for.
        /// \param counterID The counter index to get the result for.
        /// \param result The value which will contain the counter result upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetSampleUInt64( gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterID, gpa_uint64* result );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetSampleUInt64(gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterID, out gpa_uint64 result);

        /// \brief Get a sample of type 32-bit unsigned integer.
        ///
        /// This function will block until the value is available.
        /// Use GPA_IsSampleReady if you do not wish to block.
        /// \param sessionID The session identifier with the sample you wish to retreive the result of.
        /// \param sampleID The identifier of the sample to get the result for.
        /// \param counterIndex The counter index to get the result for.
        /// \param result The value which will contain the counter result upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetSampleUInt32( gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterIndex, gpa_uint32* result );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetSampleUInt32(gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterIndex, out gpa_uint32 result);

        /// \brief Get a sample of type 64-bit float.
        ///
        /// This function will block until the value is available.
        /// Use GPA_IsSampleReady if you do not wish to block.
        /// \param sessionID The session identifier with the sample you wish to retreive the result of.
        /// \param sampleID The identifier of the sample to get the result for.
        /// \param counterIndex The counter index to get the result for.
        /// \param result The value which will contain the counter result upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetSampleFloat64( gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterIndex, gpa_float64* result );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetSampleFloat64(gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterIndex, out gpa_float64 result);

        /// \brief Get a sample of type 32-bit float.
        ///
        /// This function will block until the value is available.
        /// Use GPA_IsSampleReady if you do not wish to block.
        /// \param sessionID The session identifier with the sample you wish to retreive the result of.
        /// \param sampleID The identifier of the sample to get the result for.
        /// \param counterIndex The counter index to get the result for.
        /// \param result The value which will contain the counter result upon successful execution.
        /// \return The GPA result status of the operation. GPA_STATUS_OK is returned if the operation is successful.
        /// GPALIB_DECL GPA_Status GPA_GetSampleFloat32( gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterIndex, gpa_float32* result );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern GPA_Status GPA_GetSampleFloat32(gpa_uint32 sessionID, gpa_uint32 sampleID, gpa_uint32 counterIndex, out gpa_float32 result);

        /// \brief Get a string translation of a GPA status value.
        ///
        /// Provides a simple method to convert a status enum value into a string which can be used to display log messages.
        /// \param status The status to convert into a string.
        /// \return A string which describes the supplied status.
        /// GPALIB_DECL const char* GPA_GetStatusAsStr( GPA_Status status );
        [DllImport("GPUPerfAPIGL.dll")]
        public static extern string GPA_GetStatusAsStr(GPA_Status status);
    }
}
