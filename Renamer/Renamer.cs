/*
Copyright (c) 2014~2015, Justin Bengtson
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
using System.Linq;

namespace regexKSP {
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class KerbalRenamer : MonoBehaviour {
	    void Start() {
            ConfigNode data = null;
            foreach(ConfigNode node in GameDatabase.Instance.GetConfigNodes("KERBALRENAMER")) {
                data = node;
			}
            if(data == null) { return; }

			// We use this method so that we're not constantly loading node from the GameDatabase.
			KerbalRenamerData.instance = new KerbalRenamerData(data);
	    }
	}

	public class KerbalRenamerData {
		public static KerbalRenamerData instance;

		private float badassPercent = 0.05f;
		private float femalePercent = 0.5f;
		private bool useBellCurveMethod = true;
		private bool dontInsultMe = false;
		private bool preserveOriginals = false;
		private bool generateNewStats = true;
// new
		private string cultureDescriptor = "Culture";
// end new
		private Culture[] cultures = {};

		public KerbalRenamerData(ConfigNode data) {
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
// new
			if(data.HasValue("cultureDescriptor")) {
				cultureDescriptor = data.GetValue("cultureDescriptor");
			}
// end new
			ConfigNode[] cultureclub = data.GetNodes("Culture");
			for(int i = 0; i < cultureclub.Length; i++) {
				Culture c = new Culture(cultureclub[i]);
				ctemp.Add(c);
			}
			cultures = ctemp.ToArray();

	        GameEvents.onKerbalAdded.Add(new EventData<ProtoCrewMember>.OnEvent(OnKerbalAdded));
			GameEvents.onGUIAstronautComplexSpawn.Add(new EventVoid.OnEvent(OnGUIAstronautComplexSpawn));
		}

	    public void OnKerbalAdded(ProtoCrewMember kerbal) {
			if(preserveOriginals) {
				if(kerbal.name == "Jebediah Kerman" || kerbal.name == "Bill Kerman" || kerbal.name == "Bob Kerman" || kerbal.name == "Valentina Kerman") {
					return;
				}
			}
	        UnityEngine.Random.seed = (int)(System.DateTime.Now.Millisecond * kerbal.name.GetHashCode());

			if(generateNewStats) {
				if(kerbal.type == ProtoCrewMember.KerbalType.Crew || kerbal.type == ProtoCrewMember.KerbalType.Applicant) {
					// generate some new stats
					kerbal.stupidity = rollStupidity();
					kerbal.courage = rollCourage();
					kerbal.isBadass = (UnityEngine.Random.Range(0.0f, 1.0f) < badassPercent);
	
					float rand = UnityEngine.Random.Range(0.0f, 1.0f);
					if(rand < 0.33f) {
						KerbalRoster.SetExperienceTrait(kerbal, "Pilot");
					} else if(rand < 0.66f) {
						KerbalRoster.SetExperienceTrait(kerbal, "Engineer");
					} else {
						KerbalRoster.SetExperienceTrait(kerbal, "Scientist");
					}
	
					if(UnityEngine.Random.Range(0.0f, 1.0f) < femalePercent) {
						kerbal.gender = ProtoCrewMember.Gender.Female;
					} else {
						kerbal.gender = ProtoCrewMember.Gender.Male;
					}
				}
			}

			string name = this.getName(kerbal);
			if(name.Length > 0) {
	        	kerbal.name = name;
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
// new
					if(parent.cultureName.Length > 0) {
						c.flightLog.AddEntryUnique(new FlightLog.Entry(0, cultureDescriptor, parent.cultureName));
					}
// end new
					return firstName + " " + lastName;
				} else {
// new
					if(parent.cultureName.Length > 0) {
						c.flightLog.AddEntryUnique(new FlightLog.Entry(0, cultureDescriptor, parent.cultureName));
					}
// end new
					return lastName;
				}
			} else {
				// 0 length names should be handled elsewhere.
				return firstName;
			}
	    }

// new
		public void OnGUIAstronautComplexSpawn() {
			CrewItemContainer cic;
			CMScrollList scroll;

			UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(typeof(CMAstronautComplex));
			if(objs.Length < 1) { return; }
			CMAstronautComplex complex = (CMAstronautComplex) objs[0];
			FieldInfo[] scrolls = typeof(CMAstronautComplex).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(c => c.FieldType == typeof(CMScrollList)).ToArray();

			for(int i = 0; i < scrolls.Length; i++) {
				scroll = (CMScrollList) scrolls[i].GetValue(complex);
				for(int j = 0; j < ((UIScrollList) scroll).Count; j++) {
					UIListItemContainer listItemContainer = (UIListItemContainer) ((UIScrollList) scroll).GetItem(j);
					cic = (CrewItemContainer) listItemContainer.GetComponent<CrewItemContainer>();
					FlightLog.Entry flight = cic.GetCrewRef().flightLog.Entries.FirstOrDefault(e => e.type == cultureDescriptor);
					if(flight != null) {
						FieldInfo fi = typeof(CrewItemContainer).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(c => c.FieldType == typeof(Renderer));
Debug.Log("KerbalRenamer: Cast to Renderer");
						Renderer foo = (Renderer) fi.GetValue(cic);
						Culture culture = getCultureByName(flight.target);
						if(culture != null) {
Debug.Log("KerbalRenamer: Cast to Texture");
							foo.material.mainTexture = (Texture) culture.cultureTex;
						}
					}
				}
			}
		}
// end new

		private Culture getCultureByName(string name) {
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

	public class Culture {
		public bool femaleSurnamesExist = false;
		public string cultureName = "";
		public Texture cultureTex = null;
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
				cultureTex = GameDatabase.Instance.GetTexture("KerbalRenamer/Icons/" + cultureName, false);
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