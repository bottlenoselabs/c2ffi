﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiTargetPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>>? _ImmutableArrayCEnumValue;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>> ImmutableArrayCEnumValue
        #nullable enable annotations
        {
            get => _ImmutableArrayCEnumValue ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>>)Options.GetTypeInfo(typeof(global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>> Create_ImmutableArrayCEnumValue(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>> jsonTypeInfo))
            {
                var info = new global::System.Text.Json.Serialization.Metadata.JsonCollectionInfoValues<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>>
                {
                    ObjectCreator = () => new global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>(),
                    SerializeHandler = null
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateImmutableEnumerableInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CEnumValue>, global::c2ffi.Data.Nodes.CEnumValue>(options, info, createRangeFunc: global::System.Collections.Immutable.ImmutableArray.CreateRange);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }
    }
}
