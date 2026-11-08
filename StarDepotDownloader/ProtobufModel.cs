// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using ProtoBuf;
using ProtoBuf.Meta;

namespace StarDepotDownloader
{
#pragma warning disable PBN9001
    [ProtoModel]
    [ProtoSerializable(typeof(AccountSettingsStore))]
    [ProtoSerializable(typeof(DepotConfigStore))]
    [ProtoSerializable(typeof(ProtoManifest))]
    [ProtoSerializable(typeof(GenerateForAppRequest))]
    [ProtoSerializable(typeof(GenerateForAppResponse))]
    sealed partial class ProtobufModel : TypeModel
    {
    }
#pragma warning restore PBN9001
}
