﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiTargetPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<long>? _Int64;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<long> Int64
        #nullable enable annotations
        {
            get => _Int64 ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<long>)Options.GetTypeInfo(typeof(long));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<long> Create_Int64(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<long>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<long> jsonTypeInfo))
            {
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateValueInfo<long>(options, global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.Int64Converter);
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }
    }
}
