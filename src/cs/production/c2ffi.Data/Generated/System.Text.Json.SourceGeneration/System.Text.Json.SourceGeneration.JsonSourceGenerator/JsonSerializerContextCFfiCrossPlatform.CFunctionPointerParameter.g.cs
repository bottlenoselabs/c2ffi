﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiCrossPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointerParameter>? _CFunctionPointerParameter;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointerParameter> CFunctionPointerParameter
        {
            get => _CFunctionPointerParameter ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointerParameter>)Options.GetTypeInfo(typeof(global::c2ffi.Data.Nodes.CFunctionPointerParameter));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointerParameter> Create_CFunctionPointerParameter(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::c2ffi.Data.Nodes.CFunctionPointerParameter>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointerParameter> jsonTypeInfo))
            {
                var objectInfo = new global::System.Text.Json.Serialization.Metadata.JsonObjectInfoValues<global::c2ffi.Data.Nodes.CFunctionPointerParameter>
                {
                    ObjectCreator = () => new global::c2ffi.Data.Nodes.CFunctionPointerParameter(),
                    ObjectWithParameterizedConstructorCreator = null,
                    PropertyMetadataInitializer = _ => CFunctionPointerParameterPropInit(options),
                    ConstructorParameterMetadataInitializer = null,
                    SerializeHandler = null
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateObjectInfo<global::c2ffi.Data.Nodes.CFunctionPointerParameter>(options, objectInfo);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }

        private static global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[] CFunctionPointerParameterPropInit(global::System.Text.Json.JsonSerializerOptions options)
        {
            var properties = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[4];

            var info0 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionPointerParameter),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionPointerParameter)obj).Name,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionPointerParameter)obj).Name = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Name",
                JsonPropertyName = "name"
            };
            
            properties[0] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info0);

            var info1 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CTypeInfo>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionPointerParameter),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionPointerParameter)obj).TypeInfo,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionPointerParameter)obj).TypeInfo = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "TypeInfo",
                JsonPropertyName = "type"
            };
            
            properties[1] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CTypeInfo>(options, info1);

            var info2 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CNode),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CNode)obj).Comment,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CNode)obj).Comment = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Comment",
                JsonPropertyName = "comment"
            };
            
            properties[2] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info2);

            var info3 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CNode),
                Converter = null,
                Getter = null,
                Setter = null,
                IgnoreCondition = global::System.Text.Json.Serialization.JsonIgnoreCondition.Always,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Name",
                JsonPropertyName = null
            };
            
            properties[3] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info3);

            return properties;
        }
    }
}
