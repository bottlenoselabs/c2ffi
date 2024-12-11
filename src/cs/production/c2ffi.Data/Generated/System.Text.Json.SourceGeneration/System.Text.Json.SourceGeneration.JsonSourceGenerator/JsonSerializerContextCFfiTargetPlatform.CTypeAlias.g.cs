﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiTargetPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CTypeAlias>? _CTypeAlias;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CTypeAlias> CTypeAlias
        #nullable enable annotations
        {
            get => _CTypeAlias ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CTypeAlias>)Options.GetTypeInfo(typeof(global::c2ffi.Data.Nodes.CTypeAlias));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CTypeAlias> Create_CTypeAlias(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::c2ffi.Data.Nodes.CTypeAlias>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CTypeAlias> jsonTypeInfo))
            {
                var objectInfo = new global::System.Text.Json.Serialization.Metadata.JsonObjectInfoValues<global::c2ffi.Data.Nodes.CTypeAlias>
                {
                    ObjectCreator = () => new global::c2ffi.Data.Nodes.CTypeAlias(),
                    ObjectWithParameterizedConstructorCreator = null,
                    PropertyMetadataInitializer = _ => CTypeAliasPropInit(options),
                    ConstructorParameterMetadataInitializer = null,
                    ConstructorAttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CTypeAlias).GetConstructor(InstanceMemberBindingFlags, binder: null, global::System.Array.Empty<global::System.Type>(), modifiers: null),
                    SerializeHandler = null,
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateObjectInfo<global::c2ffi.Data.Nodes.CTypeAlias>(options, objectInfo);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }

        private static global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[] CTypeAliasPropInit(global::System.Text.Json.JsonSerializerOptions options)
        {
            var properties = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[4];

            var info0 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CType>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CTypeAlias),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CTypeAlias)obj).UnderlyingType,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CTypeAlias)obj).UnderlyingType = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "UnderlyingType",
                JsonPropertyName = "underlyingType",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CTypeAlias).GetProperty("UnderlyingType", InstanceMemberBindingFlags, null, typeof(global::c2ffi.Data.CType), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[0] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CType>(options, info0);
            properties[0].IsGetNullable = false;
            properties[0].IsSetNullable = false;

            var info1 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CLocation?>
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
            
            properties[1] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CLocation?>(options, info1);

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
                JsonPropertyName = "comment",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CNode).GetProperty("Comment", InstanceMemberBindingFlags, null, typeof(string), global::System.Array.Empty<global::System.Type>(), null),
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
                JsonPropertyName = null,
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CNode).GetProperty("Name", InstanceMemberBindingFlags, null, typeof(string), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[3] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info3);
            properties[3].IsGetNullable = false;
            properties[3].IsSetNullable = false;

            return properties;
        }
    }
}
