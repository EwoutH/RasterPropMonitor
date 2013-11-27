// Mental note:
// It is not entirely clear which .NET version should KSP plugins be compiled for,
// but the consensus is that 3.5 is the most appropriate because types introduced
// in 4.0 can be verified not to work. It is a fact that you can use C#4 itself
// with it with no ill effects, though -- at least all the features which rely
// on the compiler, rather than on the libraries.
// SCANsat is compiled for .NET 4.0 for some reason, which means that
// this assembly also needs to be compiled for 4.0 to link to it. Which can and probably will
// cause problems.
// I wish there were some clarity on the subject.
using SCANsat;
using UnityEngine;
using System;

namespace SCANsatRPM
{
	public class JSISCANsatRPM: InternalModule
	{
		[KSPField]
		public float screenAspect = 1;
		[KSPField]
		public int buttonUp;
		[KSPField]
		public int buttonDown = 1;
		[KSPField]
		public int buttonEnter = 2;
		[KSPField]
		public int buttonEsc = 3;
		[KSPField]
		public int maxZoom = 5;
		private int mapMode;
		private int zoomLevel = 0;
		private int screenWidth;
		private int screenHeight;
		private double mapCenterLong, mapCenterLat;
		private SCANmap mapGenerator;
		private CelestialBody orbitingBody;

		public bool MapRenderer(RenderTexture screen)
		{
			// Just in case.
			if (!HighLogic.LoadedSceneIsFlight)
				return false;
			if (screenWidth == 0 || screenHeight == 0) {
				screenWidth = screen.width;
				screenHeight = screen.height;
				InitMap();
				return false;
			}

			Graphics.Blit(mapGenerator.map, screen);
			return true;
		}

		public void ButtonProcessor(int buttonID)
		{
			if (screenWidth == 0 || screenHeight == 0)
				return;
			if (buttonID == buttonUp) {
			}
			if (buttonID == buttonDown) {
			}
			if (buttonID == buttonEnter) {
				ChangeMapMode(true);
			}
			if (buttonID == buttonEsc) {
				ChangeMapMode(false);
			}
		}

		private void ChangeMapMode(bool up)
		{
			mapMode += up ? 1 : -1;

			if (mapMode > 2)
				mapMode = 0;
			if (mapMode < 0)
				mapMode = 2;

			mapGenerator.resetMap(mapMode);
		}

		private void ChangeZoom(bool up)
		{
			int oldZoom = zoomLevel;
			zoomLevel += up ? 1 : -1;
			if (zoomLevel < 0)
				zoomLevel = 0;
			if (zoomLevel > maxZoom)
				zoomLevel = maxZoom;
			if (zoomLevel != oldZoom) {
				mapGenerator.mapscale = zoomLevel;
				mapGenerator.resetMap(mapMode);
			}
		}

		public override void OnUpdate()
		{
			if (!HighLogic.LoadedSceneIsFlight || vessel != FlightGlobals.ActiveVessel)
				return;

			if (mapGenerator != null && !mapGenerator.isMapComplete())
				mapGenerator.getPartialMap();

			if (!(CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA ||
			    CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal))
				return;

			if (UpdateCheck() || orbitingBody != vessel.mainBody) {
				orbitingBody = vessel.mainBody;
				mapGenerator.setBody(vessel.mainBody);
				mapCenterLong = FlightGlobals.ship_longitude;
				mapCenterLat = FlightGlobals.ship_latitude;
				mapGenerator.mapscale = zoomLevel;
				mapGenerator.centerAround(mapCenterLong, mapCenterLat);
				mapGenerator.resetMap(mapMode);
			}
		}

		private bool UpdateCheck()
		{
			if (mapGenerator == null)
				return false;

			if ((Math.Abs(FlightGlobals.ship_latitude - mapCenterLat) > 180 / zoomLevel / 2) ||
			    (Math.Abs(FlightGlobals.ship_longitude - mapCenterLong) > 360 / zoomLevel / 2))
				return true;

			return false;
		}

		private void InitMap()
		{
			mapGenerator = new SCANmap();
			mapGenerator.setSize(screenWidth, screenHeight);
			mapGenerator.setProjection(SCANmap.MapProjection.Rectangular);
			orbitingBody = vessel.mainBody;
			mapGenerator.setBody(orbitingBody);
		}
	}
}

