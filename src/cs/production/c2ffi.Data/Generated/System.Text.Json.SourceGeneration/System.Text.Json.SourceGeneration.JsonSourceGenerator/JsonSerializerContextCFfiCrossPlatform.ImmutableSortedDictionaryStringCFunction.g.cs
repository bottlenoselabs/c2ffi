﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiCrossPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>>? _ImmutableSortedDictionaryStringCFunction;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>> ImmutableSortedDictionaryStringCFunction
        #nullable enable annotations
        {
            get => _ImmutableSortedDictionaryStringCFunction ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>>)Options.GetTypeInfo(typeof(global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>> Create_ImmutableSortedDictionaryStringCFunction(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>> jsonTypeInfo))
            {
                var info = new global::System.Text.Json.Serialization.Metadata.JsonCollectionInfoValues<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>>
                {
                    ObjectCreator = null,
                    SerializeHandler = null
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateImmutableDictionaryInfo<global::System.Collections.Immutable.ImmutableSortedDictionary<string, global::c2ffi.Data.Nodes.CFunction>, string, global::c2ffi.Data.Nodes.CFunction>(options, info, createRangeFunc: global::System.Collections.Immutable.ImmutableSortedDictionary.CreateRange);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }
    }
}
