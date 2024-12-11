﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiCrossPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>>? _ImmutableArrayCFunctionPointerParameter;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>> ImmutableArrayCFunctionPointerParameter
        #nullable enable annotations
        {
            get => _ImmutableArrayCFunctionPointerParameter ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>>)Options.GetTypeInfo(typeof(global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>> Create_ImmutableArrayCFunctionPointerParameter(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>> jsonTypeInfo))
            {
                var info = new global::System.Text.Json.Serialization.Metadata.JsonCollectionInfoValues<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>>
                {
                    ObjectCreator = () => new global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>(),
                    SerializeHandler = null
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateImmutableEnumerableInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>, global::c2ffi.Data.Nodes.CFunctionPointerParameter>(options, info, createRangeFunc: global::System.Collections.Immutable.ImmutableArray.CreateRange);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }
    }
}
