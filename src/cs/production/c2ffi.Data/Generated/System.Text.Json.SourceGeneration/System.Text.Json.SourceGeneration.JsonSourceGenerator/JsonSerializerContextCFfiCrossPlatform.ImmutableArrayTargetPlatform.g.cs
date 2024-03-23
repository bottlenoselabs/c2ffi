﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiCrossPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>>? _ImmutableArrayTargetPlatform;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>> ImmutableArrayTargetPlatform
        {
            get => _ImmutableArrayTargetPlatform ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>>)Options.GetTypeInfo(typeof(global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>> Create_ImmutableArrayTargetPlatform(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>> jsonTypeInfo))
            {
                var info = new global::System.Text.Json.Serialization.Metadata.JsonCollectionInfoValues<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>>
                {
                    ObjectCreator = () => new global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>(),
                    SerializeHandler = null
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateImmutableEnumerableInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.TargetPlatform>, global::c2ffi.Data.TargetPlatform>(options, info, createRangeFunc: global::System.Collections.Immutable.ImmutableArray.CreateRange);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }
    }
}