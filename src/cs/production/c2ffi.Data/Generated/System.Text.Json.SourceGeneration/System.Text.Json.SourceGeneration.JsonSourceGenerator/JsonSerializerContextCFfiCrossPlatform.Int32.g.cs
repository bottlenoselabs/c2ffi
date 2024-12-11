﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiCrossPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<int>? _Int32;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<int> Int32
        #nullable enable annotations
        {
            get => _Int32 ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<int>)Options.GetTypeInfo(typeof(int));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<int> Create_Int32(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<int>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<int> jsonTypeInfo))
            {
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateValueInfo<int>(options, global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.Int32Converter);
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }
    }
}
