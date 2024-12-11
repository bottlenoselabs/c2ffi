﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiTargetPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionParameter>? _CFunctionParameter;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionParameter> CFunctionParameter
        #nullable enable annotations
        {
            get => _CFunctionParameter ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionParameter>)Options.GetTypeInfo(typeof(global::c2ffi.Data.Nodes.CFunctionParameter));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionParameter> Create_CFunctionParameter(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::c2ffi.Data.Nodes.CFunctionParameter>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionParameter> jsonTypeInfo))
            {
                var objectInfo = new global::System.Text.Json.Serialization.Metadata.JsonObjectInfoValues<global::c2ffi.Data.Nodes.CFunctionParameter>
                {
                    ObjectCreator = () => new global::c2ffi.Data.Nodes.CFunctionParameter(),
                    ObjectWithParameterizedConstructorCreator = null,
                    PropertyMetadataInitializer = _ => CFunctionParameterPropInit(options),
                    ConstructorParameterMetadataInitializer = null,
                    ConstructorAttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionParameter).GetConstructor(InstanceMemberBindingFlags, binder: null, global::System.Array.Empty<global::System.Type>(), modifiers: null),
                    SerializeHandler = null,
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateObjectInfo<global::c2ffi.Data.Nodes.CFunctionParameter>(options, objectInfo);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }

        private static global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[] CFunctionParameterPropInit(global::System.Text.Json.JsonSerializerOptions options)
        {
            var properties = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[5];

            var info0 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionParameter),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionParameter)obj).Name,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionParameter)obj).Name = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Name",
                JsonPropertyName = "name",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionParameter).GetProperty("Name", InstanceMemberBindingFlags, null, typeof(string), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[0] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info0);
            properties[0].IsGetNullable = false;
            properties[0].IsSetNullable = false;

            var info1 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CType>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionParameter),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionParameter)obj).Type,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionParameter)obj).Type = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Type",
                JsonPropertyName = "type",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionParameter).GetProperty("Type", InstanceMemberBindingFlags, null, typeof(global::c2ffi.Data.CType), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[1] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CType>(options, info1);
            properties[1].IsGetNullable = false;
            properties[1].IsSetNullable = false;

            var info2 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CLocation?>
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
                JsonPropertyName = "location",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CNodeWithLocation).GetProperty("Location", InstanceMemberBindingFlags, null, typeof(global::c2ffi.Data.CLocation?), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[2] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CLocation?>(options, info2);

            var info3 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
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
                JsonPropertyName = "comment",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CNode).GetProperty("Comment", InstanceMemberBindingFlags, null, typeof(string), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[3] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info3);

            var info4 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
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
                JsonPropertyName = null,
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CNode).GetProperty("Name", InstanceMemberBindingFlags, null, typeof(string), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[4] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info4);
            properties[4].IsGetNullable = false;
            properties[4].IsSetNullable = false;

            return properties;
        }
    }
}
