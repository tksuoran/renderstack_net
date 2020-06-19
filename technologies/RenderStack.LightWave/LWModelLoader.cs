#if false

/*static*/ string_to_LWModel                 LWModelLoader::loaded_models;
/*static*/ std::map<LWModel*,ulong> LWModelLoader::model_counters;

/*static*/ string_to_LWModel& LWModelLoader::getLoadedModels()
{
    return loaded_models;
}

/*static*/ Model *LWModelLoader::loadModel( const std::string &name, int layer ){
    string_to_LWModel::iterator  m_it     = loaded_models.find( name );
    LWModel                     *lw_model = NULL;
    if( m_it != loaded_models.end() ){
        dmsg( M_LWS, "Using already loaded lw model for %s", name.c_str() );
        lw_model = (*m_it).second;
    }else{
        dmsg( M_LWS, "Loading lw model %s for first time", name.c_str() );
        lw_model = new LWModel( name, 0 );
        loaded_models.insert( std::make_pair(name,lw_model) );
    }

    std::map<LWModel*,ulong>::iterator l_it = model_counters.find( lw_model );
    if( l_it == model_counters.end() ){
        //  add to map
        model_counters.insert( std::make_pair(lw_model,1) );
    }else{
        // increase count
        (*l_it).second++;
        dmsg( M_LWS, "Reuse count %ul", (*l_it).second );
    }

    std::ostringstream model_name_stream;
    model_name_stream << name; //  FIX add number
    std::string model_name = model_name_stream.str();

    if( layer == -1 )
    {
        return new Model( model_name, lw_model );
    }
    else
    {
        LWLayer *lw_layer = lw_model.getLayer( layer );
        Model   *model    = lw_layer.getModel();
        return new Model( model_name, model );
    }
}


};  //  namespace Teddy


#endif