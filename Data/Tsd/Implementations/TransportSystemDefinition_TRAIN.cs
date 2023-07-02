﻿using Klyte.Commons.UI.SpriteNames;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TRAIN = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTrain,
            VehicleInfo.VehicleType.Train,
            TransportInfo.TransportType.Train,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerTrain },
            new Color32(250, 104, 0, 255),
            240,
            LineIconSpriteNames.K45_CircleIcon,
            true,
            ItemClass.Level.Level1);
    }

}