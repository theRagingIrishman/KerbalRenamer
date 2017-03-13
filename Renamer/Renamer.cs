/*
Copyright (c) 2014~2016, Justin Bengtson
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

  Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

  Redistributions in binary form must reproduce the above copyright notice, this
  list of conditions and the following disclaimer in the documentation and/or
  other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

namespace regexKSP {
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	public class KerbalRenamer : MonoBehaviour {
		public static KerbalRenamer rInstance = null;
		private float badassPercent = 0.05f;
		private float femalePercent = 0.5f;
		private bool useBellCurveMethod = true;
		private bool dontInsultMe = false;
		private bool preserveOriginals = false;
		private bool generateNewStats = true;
		public string cultureDescriptor = "Culture";
		private Culture[] cultures = {};

	    public static KerbalRenamer Instance {
	        get {
				if((object)rInstance == null) {
					rInstance = (new GameObject("RenamerContainer")).AddComponent<KerbalRenamer>();
				}
	            return rInstance;
	        }
	    }

		public void Awake() {
			DontDestroyOnLoad(this);

            ConfigNode data = null;
            foreach(ConfigNode node in GameDatabase.Instance.GetConfigNodes("KERBALRENAMER")) {
                data = node;
			}
            if((object)data == null) {
				Debug.Log("KerbalRenamer: No config file found, thanks for playing.");
				return;
			}

			List<Culture> ctemp = new List<Culture>();
			if(data.HasValue("badassPercent")) {
				float ftemp = 0.0f;
				if(float.TryParse(data.GetValue("badassPercent"), out ftemp)) {
					badassPercent = ftemp;
				}
			}
			if(data.HasValue("femalePercent")) {
				float ftemp = 0.0f;
				if(float.TryParse(data.GetValue("femalePercent"), out ftemp)) {
					femalePercent = ftemp;
				}
			}
			if(data.HasValue("useBellCurveMethod")) {
				bool btemp = true;
				if(bool.TryParse(data.GetValue("useBellCurveMethod"), out btemp)) {
					useBellCurveMethod = btemp;
				}
			}
			if(data.HasValue("dontInsultMe")) {
				bool btemp = true;
				if(bool.TryParse(data.GetValue("dontInsultMe"), out btemp)) {
					dontInsultMe = btemp;
				}
			}
			if(data.HasValue("preserveOriginals")) {
				bool btemp = true;
				if(bool.TryParse(data.GetValue("preserveOriginals"), out btemp)) {
					preserveOriginals = btemp;
				}
			}
			if(data.HasValue("generateNewStats")) {
				bool btemp = true;
				if(bool.TryParse(data.GetValue("generateNewStats"), out btemp)) {
					generateNewStats = btemp;
				}
			}
			if(data.HasValue("cultureDescriptor")) {
				cultureDescriptor = data.GetValue("cultureDescriptor");
			}
			ConfigNode[] cultureclub = data.GetNodes("Culture");
			for(int i = 0; i < cultureclub.Length; i++) {
				Culture c = new Culture(cultureclub[i]);
				ctemp.Add(c);
			}
			cultures = ctemp.ToArray();

	        GameEvents.onKerbalAdded.Add(new EventData<ProtoCrewMember>.OnEvent(OnKerbalAdded));
		}

	    public void OnKerbalAdded(ProtoCrewMember kerbal)
        {

			List<string> originalNames = new List<string>();
			originalNames.Add("Jebediah Kerman");
			originalNames.Add("Bill Kerman");
			originalNames.Add("Bob Kerman");
			originalNames.Add("Valentina Kerman");
            if (preserveOriginals)
            {
				if (originalNames.Contains(kerbal.name)) 
                {
                    return;
                }
            }
            else // see if any of the originals are still around
            {
				foreach (var originalKerbalName in originalNames)
				{

					if (HighLogic.CurrentGame.CrewRoster[originalKerbalName] != null)
					{
						var origKerbal = HighLogic.CurrentGame.CrewRoster[originalKerbalName];
						var origTrait = origKerbal.trait;
						RerollKerbal(origKerbal);
						KerbalRoster.SetExperienceTrait(origKerbal, origTrait);
					}
				}
            }

            RerollKerbal(kerbal);
        }

        private void RerollKerbal(ProtoCrewMember kerbal)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond * kerbal.name.GetHashCode());

            if (generateNewStats)
            {
                if (kerbal.type == ProtoCrewMember.KerbalType.Crew || kerbal.type == ProtoCrewMember.KerbalType.Applicant)
                {
                    // generate some new stats
                    kerbal.stupidity = rollStupidity();
                    kerbal.courage = rollCourage();
                    kerbal.isBadass = (UnityEngine.Random.Range(0.0f, 1.0f) < badassPercent);

                    float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                    if (rand < 0.33f)
                    {
                        KerbalRoster.SetExperienceTrait(kerbal, "Pilot");
                    }
                    else if (rand < 0.66f)
                    {
                        KerbalRoster.SetExperienceTrait(kerbal, "Engineer");
                    }
                    else
                    {
                        KerbalRoster.SetExperienceTrait(kerbal, "Scientist");
                    }

                    if (UnityEngine.Random.Range(0.0f, 1.0f) < femalePercent)
                    {
                        kerbal.gender = ProtoCrewMember.Gender.Female;
                    }
                    else
                    {
                        kerbal.gender = ProtoCrewMember.Gender.Male;
                    }
                }
            }

            string name = this.getName(kerbal);
            if (name.Length > 0)
            {
                kerbal.ChangeName(name);
            }
        }

	    private string getName(ProtoCrewMember c) {
	        string firstName = "";
			string lastName = "";

			Culture parent = cultures[UnityEngine.Random.Range(0, cultures.Length)];
			if(c.gender == ProtoCrewMember.Gender.Female) {
				if(parent.fnames1.Length > 0) {
			        firstName += parent.fnames1[UnityEngine.Random.Range(0, parent.fnames1.Length)];
				}
				if(parent.fnames2.Length > 0) {
			        firstName += parent.fnames2[UnityEngine.Random.Range(0, parent.fnames2.Length)];
				}
				if(parent.fnames3.Length > 0) {
			        firstName += parent.fnames3[UnityEngine.Random.Range(0, parent.fnames3.Length)];
				}
			} else {
				if(parent.mnames1.Length > 0) {
			        firstName += parent.mnames1[UnityEngine.Random.Range(0, parent.mnames1.Length)];
				}
				if(parent.mnames2.Length > 0) {
			        firstName += parent.mnames2[UnityEngine.Random.Range(0, parent.mnames2.Length)];
				}
				if(parent.mnames3.Length > 0) {
			        firstName += parent.mnames3[UnityEngine.Random.Range(0, parent.mnames3.Length)];
				}
			}
			if(parent.femaleSurnamesExist && c.gender == ProtoCrewMember.Gender.Female) {
				if(parent.flnames1.Length > 0) {
			        lastName += parent.flnames1[UnityEngine.Random.Range(0, parent.flnames1.Length)];
				}
				if(parent.flnames2.Length > 0) {
			        lastName += parent.flnames2[UnityEngine.Random.Range(0, parent.flnames2.Length)];
				}
				if(parent.flnames3.Length > 0) {
			        lastName += parent.flnames3[UnityEngine.Random.Range(0, parent.flnames3.Length)];
				}
			} else {
				if(parent.lnames1.Length > 0) {
			        lastName += parent.lnames1[UnityEngine.Random.Range(0, parent.lnames1.Length)];
				}
				if(parent.lnames2.Length > 0) {
			        lastName += parent.lnames2[UnityEngine.Random.Range(0, parent.lnames2.Length)];
				}
				if(parent.lnames3.Length > 0) {
			        lastName += parent.lnames3[UnityEngine.Random.Range(0, parent.lnames3.Length)];
				}
			}
			if(lastName.Length > 0) {
				if(firstName.Length > 0) {
					if(parent.cultureName.Length > 0) {
						c.flightLog.AddEntryUnique(new FlightLog.Entry(0, cultureDescriptor, parent.cultureName));
					}
					return firstName + " " + lastName;
				} else {
					if(parent.cultureName.Length > 0) {
						c.flightLog.AddEntryUnique(new FlightLog.Entry(0, cultureDescriptor, parent.cultureName));
					}
					return lastName;
				}
			} else {
				// 0 length names should be handled elsewhere.
				return firstName;
			}
	    }

		public Culture getCultureByName(string name) {
			for(int i = 0; i < cultures.Length; i++) {
				if(cultures[i].cultureName == name) {
					return cultures[i];
				}
			}
			return null;
		}

		private float rollCourage() {
			if(useBellCurveMethod) {
				float retval = -0.05f;
				for(int i = 0; i < 6; i++) {
					retval += UnityEngine.Random.Range(0.01f, 0.21f);
				}
				return retval;
			} else {
				return UnityEngine.Random.Range(0.0f, 1.0f);
			}
		}

		private float rollStupidity() {
			if(useBellCurveMethod) {
				float retval = -0.05f;
				int end = 6;
				if(dontInsultMe) { end = 4; }
				for(int i = 0; i < end; i++) {
					retval += UnityEngine.Random.Range(0.01f, 0.21f);
				}
				if(retval < 0.001f) { retval = 0.001f; }
				return retval;
			} else {
				return UnityEngine.Random.Range(0.0f, 1.0f);
			}
		}
	}

	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class IconChanger_SpaceCentre : IconChanger {
		public void Awake() {
			GameEvents.onGUIAstronautComplexSpawn.Add(new EventVoid.OnEvent(OnGUIAstronautComplexSpawn));
			GameEvents.onGUILaunchScreenVesselSelected.Add(new EventData<ShipTemplate>.OnEvent(OnGUILaunchScreenVesselSelected));
		}
	}

	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class IconChanger_EditorAny : IconChanger {
		public void Awake() {
			GameEvents.onEditorScreenChange.Add(new EventData<EditorScreen>.OnEvent(OnEditorScreenChange));
		}
	}

	public class IconChanger : MonoBehaviour {
		public void OnGUIAstronautComplexSpawn() {
			StartCoroutine(CallbackUtil.DelayedCallback(1, BuildAstronautComplex));
		}

		public void OnGUILaunchScreenVesselSelected(ShipTemplate t) {
			StartCoroutine(CallbackUtil.DelayedCallback(1, BuildCrewAssignmentDialogue));
		}

		public void OnEditorScreenChange(EditorScreen e) {
			if(e == EditorScreen.Crew) {
				StartCoroutine(CallbackUtil.DelayedCallback(1, BuildCrewAssignmentDialogue));
			}
		}

		public void BuildAstronautComplex() {
			KSP.UI.CrewListItem cic;
			KSP.UI.UIList scroll;

			UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(typeof(KSP.UI.Screens.AstronautComplex));
			if(objs.Length < 1) { return; }
			KSP.UI.Screens.AstronautComplex complex = (KSP.UI.Screens.AstronautComplex) objs[0];
			FieldInfo[] scrolls = typeof(KSP.UI.Screens.AstronautComplex).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(c => c.FieldType == typeof(KSP.UI.UIList)).ToArray();

			for(int i = 0; i < scrolls.Length; i++) {
				scroll = (KSP.UI.UIList) scrolls[i].GetValue(complex);
				for(int j = 0; j < scroll.Count; j++) {
					KSP.UI.UIListItem listItem = scroll.GetUilistItemAt(j);
					cic = listItem.GetComponent<KSP.UI.CrewListItem>();
					cic.AddButtonInputDelegate(new UnityAction<KSP.UI.CrewListItem.ButtonTypes, KSP.UI.CrewListItem>(RebuildAstronautComplex));
					changeKerbalIcon(cic);
				}
			}
		}

		public void BuildCrewAssignmentDialogue() {
			if((object) KSP.UI.CrewAssignmentDialog.Instance == null) {
				return;
			}
			KSP.UI.CrewAssignmentDialog dialogue = KSP.UI.CrewAssignmentDialog.Instance;
			KSP.UI.CrewListItem cic;

			for(int j = 0; j < dialogue.scrollListAvail.Count; j++) {
				KSP.UI.UIListItem listItem = dialogue.scrollListAvail.GetUilistItemAt(j);
				cic = listItem.GetComponent<KSP.UI.CrewListItem>();
				cic.AddButtonInputDelegate(new UnityAction<KSP.UI.CrewListItem.ButtonTypes, KSP.UI.CrewListItem>(RebuildCrewAssignmentDialogue));
				changeKerbalIcon(cic);
			}
			for(int j = 0; j < dialogue.scrollListCrew.Count; j++) {
				KSP.UI.UIListItem listItem = dialogue.scrollListCrew.GetUilistItemAt(j);
				cic = listItem.GetComponent<KSP.UI.CrewListItem>();
				if((object)cic != null) {
					cic.AddButtonInputDelegate(new UnityAction<KSP.UI.CrewListItem.ButtonTypes, KSP.UI.CrewListItem>(RebuildCrewAssignmentDialogue));
					changeKerbalIcon(cic);
				}
			}
		}

		public void RebuildAstronautComplex(KSP.UI.CrewListItem.ButtonTypes type, KSP.UI.CrewListItem cic) {
			StartCoroutine(CallbackUtil.DelayedCallback(1, BuildAstronautComplex));
		}

		public void RebuildCrewAssignmentDialogue(KSP.UI.CrewListItem.ButtonTypes type, KSP.UI.CrewListItem cic) {
			StartCoroutine(CallbackUtil.DelayedCallback(1, BuildCrewAssignmentDialogue));
		}

		private void changeKerbalIcon(KSP.UI.CrewListItem cic) {
			if((object)cic.GetCrewRef() == null) {
				return;
			}
			FlightLog.Entry flight = cic.GetCrewRef().flightLog.Entries.FirstOrDefault(e => e.type == KerbalRenamer.Instance.cultureDescriptor);
			if((object)flight != null) {
				FieldInfo fi = typeof(KSP.UI.CrewListItem).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(c => c.FieldType == typeof(RawImage));
				RawImage foo = (RawImage) fi.GetValue(cic);
				Culture culture = KerbalRenamer.Instance.getCultureByName(flight.target);
				if((object)culture != null) {
					foo.texture = (Texture) GameDatabase.Instance.GetTexture("KerbalRenamer/Icons/" + culture.cultureName, false);
				}
			}
		}
	}

	public class Culture {
		public bool femaleSurnamesExist = false;
		public string cultureName = "";
		public string[] fnames1 = {};
		public string[] fnames2 = {};
		public string[] fnames3 = {};
		public string[] mnames1 = {};
		public string[] mnames2 = {};
		public string[] mnames3 = {};
		public string[] lnames1 = {};
		public string[] lnames2 = {};
		public string[] lnames3 = {};
		public string[] flnames1 = {};
		public string[] flnames2 = {};
		public string[] flnames3 = {};

		public Culture(ConfigNode node) {
			ConfigNode temp;
			string[] vals;
			if(node.HasValue("name")) {
				cultureName = node.GetValue("name");
			}
			if(node.HasNode("FFIRSTNAME1")) {
				temp = node.GetNode("FFIRSTNAME1");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					fnames1 = vals;
				}
			}
			if(node.HasNode("FFIRSTNAME2")) {
				temp = node.GetNode("FFIRSTNAME2");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					fnames2 = vals;
				}
			}
			if(node.HasNode("FFIRSTNAME3")) {
				temp = node.GetNode("FFIRSTNAME3");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					fnames3 = vals;
				}
			}
			if(node.HasNode("MFIRSTNAME1")) {
				temp = node.GetNode("MFIRSTNAME1");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					mnames1 = vals;
				}
			}
			if(node.HasNode("MFIRSTNAME2")) {
				temp = node.GetNode("MFIRSTNAME2");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					mnames2 = vals;
				}
			}
			if(node.HasNode("MFIRSTNAME3")) {
				temp = node.GetNode("MFIRSTNAME3");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					mnames3 = vals;
				}
			}
			if(node.HasNode("LASTNAME1")) {
				temp = node.GetNode("LASTNAME1");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					lnames1 = vals;
				}
			}
			if(node.HasNode("LASTNAME2")) {
				temp = node.GetNode("LASTNAME2");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					lnames2 = vals;
				}
			}
			if(node.HasNode("LASTNAME3")) {
				temp = node.GetNode("LASTNAME3");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					lnames3 = vals;
				}
			}
			if(node.HasNode("FLASTNAME1")) {
				temp = node.GetNode("FLASTNAME1");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					flnames1 = vals;
					femaleSurnamesExist = true;
				}
			}
			if(node.HasNode("FLASTNAME2")) {
				temp = node.GetNode("FLASTNAME2");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					flnames2 = vals;
					femaleSurnamesExist = true;
				}
			}
			if(node.HasNode("FLASTNAME3")) {
				temp = node.GetNode("FLASTNAME3");
				vals = temp.GetValues("key");
				if(vals.Length > 0) {
					flnames3 = vals;
					femaleSurnamesExist = true;
				}
			}
		}
	}
}