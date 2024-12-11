﻿// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace c2ffi.Data.Serialization
{
    public partial class JsonSerializerContextCFfiTargetPlatform
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointer>? _CFunctionPointer;
        
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        #nullable disable annotations // Marking the property type as nullable-oblivious.
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointer> CFunctionPointer
        #nullable enable annotations
        {
            get => _CFunctionPointer ??= (global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointer>)Options.GetTypeInfo(typeof(global::c2ffi.Data.Nodes.CFunctionPointer));
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointer> Create_CFunctionPointer(global::System.Text.Json.JsonSerializerOptions options)
        {
            if (!TryGetTypeInfoForRuntimeCustomConverter<global::c2ffi.Data.Nodes.CFunctionPointer>(options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::c2ffi.Data.Nodes.CFunctionPointer> jsonTypeInfo))
            {
                var objectInfo = new global::System.Text.Json.Serialization.Metadata.JsonObjectInfoValues<global::c2ffi.Data.Nodes.CFunctionPointer>
                {
                    ObjectCreator = () => new global::c2ffi.Data.Nodes.CFunctionPointer(),
                    ObjectWithParameterizedConstructorCreator = null,
                    PropertyMetadataInitializer = _ => CFunctionPointerPropInit(options),
                    ConstructorParameterMetadataInitializer = null,
                    ConstructorAttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionPointer).GetConstructor(InstanceMemberBindingFlags, binder: null, global::System.Array.Empty<global::System.Type>(), modifiers: null),
                    SerializeHandler = null,
                };
                
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateObjectInfo<global::c2ffi.Data.Nodes.CFunctionPointer>(options, objectInfo);
                jsonTypeInfo.NumberHandling = null;
            }
        
            jsonTypeInfo.OriginatingResolver = this;
            return jsonTypeInfo;
        }

        private static global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[] CFunctionPointerPropInit(global::System.Text.Json.JsonSerializerOptions options)
        {
            var properties = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[7];

            var info0 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CType>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionPointer),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).Type,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).Type = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Type",
                JsonPropertyName = "type",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionPointer).GetProperty("Type", InstanceMemberBindingFlags, null, typeof(global::c2ffi.Data.CType), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[0] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CType>(options, info0);
            properties[0].IsGetNullable = false;
            properties[0].IsSetNullable = false;

            var info1 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CType>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionPointer),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).ReturnType,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).ReturnType = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "ReturnType",
                JsonPropertyName = "return_type",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionPointer).GetProperty("ReturnType", InstanceMemberBindingFlags, null, typeof(global::c2ffi.Data.CType), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[1] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CType>(options, info1);
            properties[1].IsGetNullable = false;
            properties[1].IsSetNullable = false;

            var info2 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionPointer),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).Parameters,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).Parameters = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "Parameters",
                JsonPropertyName = "parameters",
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionPointer).GetProperty("Parameters", InstanceMemberBindingFlags, null, typeof(global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[2] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::System.Collections.Immutable.ImmutableArray<global::c2ffi.Data.Nodes.CFunctionPointerParameter>>(options, info2);

            var info3 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CFunctionCallingConvention>
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::c2ffi.Data.Nodes.CFunctionPointer),
                Converter = null,
                Getter = static obj => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).CallingConvention,
                Setter = static (obj, value) => ((global::c2ffi.Data.Nodes.CFunctionPointer)obj).CallingConvention = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = null,
                PropertyName = "CallingConvention",
                JsonPropertyName = null,
                AttributeProviderFactory = static () => typeof(global::c2ffi.Data.Nodes.CFunctionPointer).GetProperty("CallingConvention", InstanceMemberBindingFlags, null, typeof(global::c2ffi.Data.CFunctionCallingConvention), global::System.Array.Empty<global::System.Type>(), null),
            };
            
            properties[3] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CFunctionCallingConvention>(options, info3);

            var info4 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::c2ffi.Data.CLocation?>
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
            
            properties[4] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::c2ffi.Data.CLocation?>(options, info4);

            var info5 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
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
            
            properties[5] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info5);

            var info6 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<string>
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
            
            properties[6] = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<string>(options, info6);
            properties[6].IsGetNullable = false;
            properties[6].IsSetNullable = false;

            return properties;
        }
    }
}
