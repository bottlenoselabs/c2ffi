﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiTargetPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.COpaqueType>? _COpaqueType;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.COpaqueType> COpaqueType
        {
            get => _COpaqueType ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.COpaqueType>)Options.GetTypeInfo(typeof(global::c2ffi.Data.Nodes.COpaqueType));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.COpaqueType> Create_COpaqueType(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::c2ffi.Data.Nodes.COpaqueType>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.COpaqueType> jsonTypeInfo))
            {
                var objectInfo = new global::System.Text.Json.Serialization.Metadata.JsonObjectInfoValues<global::c2ffi.Data.Nodes.COpaqueType>
                {
                    ObjectCreator = () => new global::c2ffi.Data.Nodes.COpaqueType(),
                    ObjectWithParameterizedConstructorCreator = null,
                    PropertyMetadataInitializer = _ => COpaqueTypePropInit(options),
                    ConstructorParameterMetadataInitializer = null,
                    SerializeHandler = null
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateObjectInfo<global::c2ffi.Data.Nodes.COpaqueType>(options, objectInfo);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }

        private static global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[] COpaqueTypePropInit(global::System.Text.Json.JsonSerializerOptions options)
        {
            var properties = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[3];

            var info0 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CLocation?>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CNodeWithLocation),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CNodeWithLocation)obj).Location,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CNodeWithLocation)obj).Location = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Location",
                JsonPropertyName = "location"
            };
            
            properties[0] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CLocation?>(options, info0);

            var info1 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
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
            
            properties[1] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info1);

            var info2 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
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
            
            properties[2] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info2);

            return properties;
        }
    }
}
