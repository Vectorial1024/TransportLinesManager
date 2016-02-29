﻿using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    class TLMTrainModifyRedirects : BasicTransportExtension<PassengerTrainAI>
    {
        private static TLMTrainModifyRedirects _instance;
        public static TLMTrainModifyRedirects instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMTrainModifyRedirects();
                }
                return _instance;
            }
        }


        #region Hooks for PassengerTrainAI

        public void SetTransportLine(ushort vehicleID, ref Vehicle data, ushort transportLine)
        {
            var t = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine];
            TLMUtils.doLog("SetTransportLine! Prefab id: {0} ({4}), For line: {1} {2} ({3})", data.Info.m_prefabDataIndex, t.Info.m_transportType, t.m_lineNumber, transportLine, data.Info.name);
            this.RemoveLine(vehicleID, ref data);

            data.m_transportLine = transportLine;
            if (transportLine != 0)
            {

                if (t.Info.m_transportType == TransportInfo.TransportType.Train && TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.TRAIN_PREFIX) != (int)ModoNomenclatura.Nenhum)
                {

                    TLMUtils.doLog("Get prefix");
                    uint prefix = t.m_lineNumber / 1000u;

                    TLMUtils.doLog("pre getAssetListForPrefix");
                    List<string> assetsList = TLMTrainModifyRedirects.instance.getAssetListForPrefix(prefix);


                    if (!assetsList.Contains(data.Info.name))
                    {
                        TLMUtils.doLog("pre getRandomModel");
                        var randomInfo = instance.getRandomModel(prefix);
                        TLMUtils.doLog("pos getRandomModel");
                        if (randomInfo != null)
                        {
                            TLMUtils.doLog("pre data.Info = randomInfo");
                            data.Info = randomInfo;
                        }
                    }
                }
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)transportLine].AddVehicle(vehicleID, ref data, true);
            }
            else
            {
                data.m_flags |= Vehicle.Flags.GoingBack;
            }
            TLMUtils.doLog("GOTO StartPathFindFake?");
            if (!this.StartPathFind(vehicleID, ref data))
            {
                data.Unspawn(vehicleID);
            }
        }
        private void RemoveLine(ushort vehicleID, ref Vehicle data)
        {

            TLMUtils.doLog("RemoveLine??? WHYYYYYYY!?");
            if (data.m_transportLine != 0)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)data.m_transportLine].RemoveVehicle(vehicleID, ref data);
                data.m_transportLine = 0;
            }
        }

        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            TLMUtils.doLog("StartPathFind!!!!!??? AEHOOO!");
            ExtraVehiclesStats.OnVehicleStop(vehicleID, vehicleData);
            //ORIGINAL
            if (vehicleData.m_leadingVehicle == 0)
            {
                Vector3 startPos;
                if ((vehicleData.m_flags & Vehicle.Flags.Reversed) != Vehicle.Flags.None)
                {
                    ushort lastVehicle = vehicleData.GetLastVehicle(vehicleID);
                    startPos = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)lastVehicle].m_targetPos0;
                }
                else
                {
                    startPos = vehicleData.m_targetPos0;
                }
                if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    if (vehicleData.m_sourceBuilding != 0)
                    {
                        Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding].m_position;
                        return this.StartPathFind(vehicleID, ref vehicleData, startPos, position);
                    }
                }
                else if ((vehicleData.m_flags & Vehicle.Flags.DummyTraffic) != Vehicle.Flags.None)
                {
                    if (vehicleData.m_targetBuilding != 0)
                    {
                        Vector3 position2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding].m_position;
                        return this.StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position2);
                    }
                }
                else if (vehicleData.m_targetBuilding != 0)
                {
                    Vector3 position3 = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)vehicleData.m_targetBuilding].m_position;
                    return this.StartPathFind(vehicleID, ref vehicleData, startPos, position3);
                }
            }
            return false;
        }

        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 v4, Vector3 v3) { TLMUtils.doLog("StartPathFind??? WHYYYYYYY!?"); return false; }


        public void OnCreated(ILoading loading)
        {
            TLMUtils.doLog("TLMSurfaceMetroRedirects Criado!");
        }
        #endregion

        //#region Hooks for PublicTransportVehicleWorldInfoPanel
        //private void IconChanged(UIComponent comp, string text)
        //{

        //    PublicTransportVehicleWorldInfoPanel ptvwip = Singleton<PublicTransportVehicleWorldInfoPanel>.instance;
        //    ushort lineId = m_instance.TransportLine;
        //    UISprite iconSprite = ptvwip.gameObject.transform.Find("VehicleType").GetComponent<UISprite>();
        //    TLMUtils.doLog("lineId == {0}", lineId);
        //}
        //InstanceID m_instance;
        //#endregion

        public void OnReleased()
        {
        }

        #region Hooking
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();



        public void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            TLMUtils.doLog("Loading SurfaceMetro Hooks!");
            AddRedirect(typeof(PassengerTrainAI), typeof(TLMTrainModifyRedirects).GetMethod("SetTransportLine", allFlags), ref redirects);
            AddRedirect(typeof(PassengerTrainAI), typeof(TLMTrainModifyRedirects).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null), ref redirects);
            AddRedirect(typeof(TLMTrainModifyRedirects), typeof(TrainAI).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(Vector3) }, null), ref redirects);
            AddRedirect(typeof(TLMTrainModifyRedirects), typeof(PassengerTrainAI).GetMethod("RemoveLine", allFlags), ref redirects);
        }

        public void DisableHooks()
        {
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            redirects.Clear();
        }
        #endregion
    }
}
