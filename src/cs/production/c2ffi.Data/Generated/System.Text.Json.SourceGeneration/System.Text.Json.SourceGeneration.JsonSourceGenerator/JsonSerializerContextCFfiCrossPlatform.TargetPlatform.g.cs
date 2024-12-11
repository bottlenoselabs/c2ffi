﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiCrossPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.TargetPlatform>? _TargetPlatform;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.TargetPlatform> TargetPlatform
        #nullable enable annotations
        {
            get => _TargetPlatform ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.TargetPlatform>)Options.GetTypeInfo(typeof(global::c2ffi.Data.TargetPlatform));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.TargetPlatform> Create_TargetPlatform(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::c2ffi.Data.TargetPlatform>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.TargetPlatform> jsonTypeInfo))
            {
                global::System.Text.Json.Serialization.JsonConverter converter = ExpandConverter(typeof(global::c2ffi.Data.TargetPlatform), new global::c2ffi.Data.Serialization.TargetPlatformJsonConverter(), options);
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateValueInfo<global::c2ffi.Data.TargetPlatform> (options, converter);
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }
    }
}
