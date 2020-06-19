using System;

namespace example.Loading
{
    public interface ILoadingWindow : IDisposable
    {
        object SyncFormVisible { get; }

        void Span(float expectedTime, float start, float end);
        void Run();
        void Close();
        void Message(string message);

    }
}
