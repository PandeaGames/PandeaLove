using UnityEngine;
using System;
using System.Collections.Generic;

namespace I2.Loc
{
	public enum ePluralType { Zero, One, Two, Few, Many, Plural };

    public static class GoogleLanguages
    {
        public static string GetLanguageCode(string Filter, bool ShowWarnings = false)
        {
            if (string.IsNullOrEmpty(Filter))
                return string.Empty;

            string[] Filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());

            foreach (var kvp in mLanguageDef)
                if (LanguageMatchesFilter(kvp.Key, Filters))
                    return kvp.Value.Code;

            if (ShowWarnings)
                Debug.Log(string.Format("Language '{0}' not recognized. Please, add the language code to GoogleTranslation.cs", Filter));
            return string.Empty;
        }


        public static List<string> GetLanguagesForDropdown(string Filter, string CodesToExclude)
        {
            string[] Filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());

            List<string> Languages = new List<string>();

            foreach (var kvp in mLanguageDef)
                if (string.IsNullOrEmpty(Filter) || LanguageMatchesFilter(kvp.Key, Filters))
                {
                    string code = string.Concat("[" + kvp.Value.Code + "]");
                    if (!CodesToExclude.Contains(code))
                        Languages.Add(kvp.Key + " " + code);
                }

            // Add headers to variants (e.g. "English/English"  before all English variants
            for (int i = Languages.Count - 2; i >= 0; --i)
            {
                string Prefix = Languages[i].Substring(0, Languages[i].IndexOf(" ["));
                if (Languages[i + 1].StartsWith(Prefix))
                {
                    Languages[i] = Prefix + "/" + Languages[i];
                    Languages.Insert(i + 1, Prefix + "/");
                }
            }
            return Languages;
        }

        public static string GetClosestLanguage(string Filter)
        {
            if (string.IsNullOrEmpty(Filter))
                return string.Empty;

            string[] Filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());

            foreach (var kvp in mLanguageDef)
                if (LanguageMatchesFilter(kvp.Key, Filters))
                    return kvp.Key;//GetFormatedLanguageName( kvp.Key );

            return string.Empty;
        }

        // "Engl Unit" matches "English/United States"
        static bool LanguageMatchesFilter(string Language, string[] Filters)
        {
            Language = Language.ToLowerInvariant();
            for (int i = 0, imax = Filters.Length; i < imax; ++i)
                if (Filters[i] != "")
                {
                    if (!Language.Contains(Filters[i].ToLower()))
                        return false;
                    else
                        Language = Language.Remove(Language.IndexOf(Filters[i]), Filters[i].Length);
                }
            return true;
        }


        // "Arabic/Algeria [ar-XX]" returns "Arabic (Algeria)"
        // "English/English [en]" returns "English"
        public static string GetFormatedLanguageName(string Language)
        {
            string BaseLanguage = string.Empty;

            //-- Remove code --------
            int Index = Language.IndexOf(" [");
            if (Index > 0)
                Language = Language.Substring(0, Index);

            //-- Check for main language: "English/English [en]" returns "English" -----------
            Index = Language.IndexOf('/');
            if (Index > 0)
            {
                BaseLanguage = Language.Substring(0, Index);
                if (Language == (BaseLanguage + "/" + BaseLanguage))
                    return BaseLanguage;

                //-- Convert variants into right format
                Language = Language.Replace("/", " (") + ")";
            }

            return Language;
        }

        // English British   ->   "English Canada [en-CA]"
        public static string GetCodedLanguage(string Language, string code)
        {
            string DefaultCode = GetLanguageCode(Language, false);
            if (string.Compare(code, DefaultCode, StringComparison.OrdinalIgnoreCase) == 0)
                return Language;
            return string.Concat(Language, " [", code, "]");
        }

        // "English Canada [en-CA]" ->  "English Canada", "en-CA"
        public static void UnPackCodeFromLanguageName(string CodedLanguage, out string Language, out string code)
        {
            if (string.IsNullOrEmpty(CodedLanguage))
            {
                Language = string.Empty;
                code = string.Empty;
                return;
            }
            int Index = CodedLanguage.IndexOf("[");
            if (Index < 0)
            {
                Language = CodedLanguage;
                code = GetLanguageCode(Language);
            }
            else
            {
                Language = CodedLanguage.Substring(0, Index).Trim();
                code = CodedLanguage.Substring(Index + 1, CodedLanguage.IndexOf("]", Index) - Index - 1);
            }
        }

        public static string GetGoogleLanguageCode(string InternationalCode)
        {
            foreach (var kvp in mLanguageDef)
                if (InternationalCode == kvp.Value.Code)
                {
                    if (kvp.Value.GoogleCode == "-")
                        return null;
                    return (!string.IsNullOrEmpty(kvp.Value.GoogleCode)) ? kvp.Value.GoogleCode : InternationalCode;
                }

            return InternationalCode;
        }

        public static List<string> GetAllInternationalCodes()
        {
            var set = new HashSet<string>();

            foreach (var kvp in mLanguageDef)
                set.Add(kvp.Value.Code);

            return new List<string>(set);
        }

        public static bool LanguageCode_HasJoinedWord(string languageCode)
        {
            foreach (var kvp in mLanguageDef)
                if (languageCode == kvp.Value.GoogleCode || languageCode==kvp.Value.Code )
                    return kvp.Value.HasJoinedWords;

            return false;
        }

        public struct LanguageCodeDef
		{
			public string Code;		// Language International Code
			public string GoogleCode;	// Google Translator doesn't support all languages, this is the code of closest supported language
            public bool HasJoinedWords; // Some languages (e.g. Chinese, Japanese and Thai) don't add spaces to their words (all characters are placed toguether)
            public int PluralRule;
        }

        public static Dictionary<string, LanguageCodeDef> mLanguageDef = new Dictionary<string, LanguageCodeDef>(StringComparer.Ordinal)  
		{
			{"Afrikaans", 			new LanguageCodeDef(){PluralRule=1, Code="af"}},
			{"Albanian", 			new LanguageCodeDef(){PluralRule=1, Code="sq"}},
			{"Arabic", 				new LanguageCodeDef(){PluralRule=11, Code="ar"}},
			{"Arabic/Algeria", 		new LanguageCodeDef(){PluralRule=11, Code="ar-DZ", GoogleCode="ar"}},
			{"Arabic/Bahrain", 		new LanguageCodeDef(){PluralRule=11, Code="ar-BH", GoogleCode="ar"}},
			{"Arabic/Egypt", 		new LanguageCodeDef(){PluralRule=11, Code="ar-EG", GoogleCode="ar"}},
			{"Arabic/Iraq", 		new LanguageCodeDef(){PluralRule=11, Code="ar-IQ", GoogleCode="ar"}},
			{"Arabic/Jordan", 		new LanguageCodeDef(){PluralRule=11, Code="ar-JO", GoogleCode="ar"}},
			{"Arabic/Kuwait", 		new LanguageCodeDef(){PluralRule=11, Code="ar-KW", GoogleCode="ar"}},
			{"Arabic/Lebanon", 		new LanguageCodeDef(){PluralRule=11, Code="ar-LB", GoogleCode="ar"}},
			{"Arabic/Libya", 		new LanguageCodeDef(){PluralRule=11, Code="ar-LY", GoogleCode="ar"}},
			{"Arabic/Morocco", 		new LanguageCodeDef(){PluralRule=11, Code="ar-MA", GoogleCode="ar"}},
			{"Arabic/Oman", 		new LanguageCodeDef(){PluralRule=11, Code="ar-OM", GoogleCode="ar"}},
			{"Arabic/Qatar", 		new LanguageCodeDef(){PluralRule=11, Code="ar-QA", GoogleCode="ar"}},
			{"Arabic/Saudi Arabia", new LanguageCodeDef(){PluralRule=11, Code="ar-SA", GoogleCode="ar"}},
			{"Arabic/Syria", 		new LanguageCodeDef(){PluralRule=11, Code="ar-SY", GoogleCode="ar"}},
			{"Arabic/Tunisia", 		new LanguageCodeDef(){PluralRule=11, Code="ar-TN", GoogleCode="ar"}},
			{"Arabic/U.A.E.", 		new LanguageCodeDef(){PluralRule=11, Code="ar-AE", GoogleCode="ar"}},
			{"Arabic/Yemen", 		new LanguageCodeDef(){PluralRule=11, Code="ar-YE", GoogleCode="ar"}},
			{"Armenian", 			new LanguageCodeDef(){PluralRule=1, Code="hy"}},
			{"Azerbaijani", 		new LanguageCodeDef(){PluralRule=1, Code="az"}},
			{"Basque",				new LanguageCodeDef(){PluralRule=1, Code="eu"}},
			{"Basque/Spain", 		new LanguageCodeDef(){PluralRule=1, Code="eu-ES", GoogleCode="eu"}},
			{"Belarusian", 			new LanguageCodeDef(){PluralRule=5, Code="be"}},
			{"Bosnian", 			new LanguageCodeDef(){PluralRule=5, Code="bs"}},
			{"Bulgariaa", 			new LanguageCodeDef(){PluralRule=1, Code="bg"}},
			{"Catalan", 			new LanguageCodeDef(){PluralRule=1, Code="ca"}},
			{"Chinese",				new LanguageCodeDef(){PluralRule=0, Code="zh", 	GoogleCode="zh-CN", HasJoinedWords=true}},
			{"Chinese/Hong Kong",	new LanguageCodeDef(){PluralRule=0, Code="zh-HK", GoogleCode="zh-TW", HasJoinedWords=true}},
			{"Chinese/Macau", 		new LanguageCodeDef(){PluralRule=0, Code="zh-MO", GoogleCode="zh-CN", HasJoinedWords=true}},
			{"Chinese/PRC", 		new LanguageCodeDef(){PluralRule=0, Code="zh-CN", GoogleCode="zh-CN", HasJoinedWords=true}},
			{"Chinese/Simplified", 	new LanguageCodeDef(){PluralRule=0, Code="zh-CN", GoogleCode="zh-CN", HasJoinedWords=true}},
			{"Chinese/Singapore", 	new LanguageCodeDef(){PluralRule=0, Code="zh-SG", GoogleCode="zh-CN", HasJoinedWords=true}},
			{"Chinese/Taiwan", 		new LanguageCodeDef(){PluralRule=0, Code="zh-TW", GoogleCode="zh-TW", HasJoinedWords=true}},
			{"Chinese/Traditional", new LanguageCodeDef(){PluralRule=0, Code="zh-TW", GoogleCode="zh-TW", HasJoinedWords=true}},
			{"Croatian", 			new LanguageCodeDef(){PluralRule=5, Code="hr"}},
			{"Croatian/Bosnia and Herzegovina", new LanguageCodeDef(){PluralRule=5, Code="hr-BA", GoogleCode="hr"}},
			{"Czech", 				new LanguageCodeDef(){PluralRule=7, Code="cs"}},
			{"Danish", 				new LanguageCodeDef(){PluralRule=1, Code="da"}},
			//{"Dhivehi", new LanguageCodeDef(){Code="diV"}},		//---------------
			//{"Divehi", new LanguageCodeDef(){Code="dv"}},		//---------------
			{"Dutch", 				new LanguageCodeDef(){PluralRule=1, Code="nl"}},
			{"Dutch/Belgium", 		new LanguageCodeDef(){PluralRule=1, Code="nl-BE", GoogleCode="nl"}},
			{"Dutch/Netherlands", 	new LanguageCodeDef(){PluralRule=1, Code="nl-NL", GoogleCode="nl"}},
			{"English", 			new LanguageCodeDef(){PluralRule=1, Code="en"}},
			{"English/Australia", 	new LanguageCodeDef(){PluralRule=1, Code="en-AU", GoogleCode="en"}},
			{"English/Belize", 		new LanguageCodeDef(){PluralRule=1, Code="en-BZ", GoogleCode="en"}},
			{"English/Canada", 		new LanguageCodeDef(){PluralRule=1, Code="en-CA", GoogleCode="en"}},
			{"English/Caribbean", 	new LanguageCodeDef(){PluralRule=1, Code="en-CB", GoogleCode="en"}},
			{"English/Ireland", 	new LanguageCodeDef(){PluralRule=1, Code="en-IE", GoogleCode="en"}},
			{"English/Jamaica", 	new LanguageCodeDef(){PluralRule=1, Code="en-JM", GoogleCode="en"}},
			{"English/New Zealand", new LanguageCodeDef(){PluralRule=1, Code="en-NZ", GoogleCode="en"}},
			{"English/Republic of the Philippines", new LanguageCodeDef(){PluralRule=1, Code="en-PH", GoogleCode="en"}},
			{"English/South Africa",new LanguageCodeDef(){PluralRule=1, Code="en-ZA", GoogleCode="en"}},
			{"English/Trinidad", 	new LanguageCodeDef(){PluralRule=1, Code="en-TT", GoogleCode="en"}},
			{"English/United Kingdom",new LanguageCodeDef(){PluralRule=1, Code="en-GB", GoogleCode="en"}},
			{"English/United States",new LanguageCodeDef(){PluralRule=1, Code="en-US", GoogleCode="en"}},
			{"English/Zimbabwe", 	new LanguageCodeDef(){PluralRule=1, Code="en-ZW", GoogleCode="en"}},
			{"Esperanto", 			new LanguageCodeDef(){PluralRule=1, Code="eo"}},
			{"Estonian", 			new LanguageCodeDef(){PluralRule=1, Code="et"}},
			{"Faeroese", 			new LanguageCodeDef(){PluralRule=1, Code="fo", GoogleCode="-"}},
			//{"Farsi", new LanguageCodeDef(){Code="fa"}},		//--------------------
			{"Filipino", 			new LanguageCodeDef(){PluralRule=2, Code="tl"}},
			{"Finnish", 			new LanguageCodeDef(){PluralRule=1, Code="fi"}},
			{"French", 				new LanguageCodeDef(){PluralRule=2, Code="fr"}},
			{"French/Belgium", 		new LanguageCodeDef(){PluralRule=2, Code="fr-BE", GoogleCode="fr"}},
			{"French/Canada", 		new LanguageCodeDef(){PluralRule=2, Code="fr-CA", GoogleCode="fr"}},
			{"French/France", 		new LanguageCodeDef(){PluralRule=2, Code="fr-FR", GoogleCode="fr"}},
			{"French/Luxembourg", 	new LanguageCodeDef(){PluralRule=2, Code="fr-LU", GoogleCode="fr"}},
			{"French/Principality of Monaco", new LanguageCodeDef(){PluralRule=2, Code="fr-MC", GoogleCode="fr"}},
			{"French/Switzerland", 	new LanguageCodeDef(){PluralRule=2, Code="fr-CH", GoogleCode="fr"}},
			//{"Gaelic", 	new LanguageCodeDef(){Code="gd"}}, //------------------
			{"Galician", 			new LanguageCodeDef(){PluralRule=1, Code="gl"}},
			{"Galician/Spain", 		new LanguageCodeDef(){PluralRule=1, Code="gl-ES", GoogleCode="gl"}},
			{"Georgian", 			new LanguageCodeDef(){PluralRule=0, Code="ka"}},
			{"German", 				new LanguageCodeDef(){PluralRule=1, Code="de"}},
			{"German/Austria", 		new LanguageCodeDef(){PluralRule=1, Code="de-AT", GoogleCode="de"}},
			{"German/Germany", 		new LanguageCodeDef(){PluralRule=1, Code="de-DE", GoogleCode="de"}},
			{"German/Liechtenstein",new LanguageCodeDef(){PluralRule=1, Code="de-LI", GoogleCode="de"}},
			{"German/Luxembourg", 	new LanguageCodeDef(){PluralRule=1, Code="de-LU", GoogleCode="de"}},
			{"German/Switzerland", 	new LanguageCodeDef(){PluralRule=1, Code="de-CH", GoogleCode="de"}},
			{"Greek", 				new LanguageCodeDef(){PluralRule=1, Code="el"}},
			{"Gujarati", 			new LanguageCodeDef(){PluralRule=1, Code="gu"}},
			{"Hebrew", 				new LanguageCodeDef(){PluralRule=1, Code="he", GoogleCode="iw"}},
			{"Hindi", 				new LanguageCodeDef(){PluralRule=1, Code="hi"}},
			{"Hungarian", 			new LanguageCodeDef(){PluralRule=1, Code="hu"}},
			{"Icelandic", 			new LanguageCodeDef(){PluralRule=14, Code="is"}},
			{"Indonesian", 			new LanguageCodeDef(){PluralRule=0, Code="id"}},
			{"Irish", 				new LanguageCodeDef(){PluralRule=10, Code="ga"}},
			{"Italian", 			new LanguageCodeDef(){PluralRule=1, Code="it"}},
			{"Italian/Italy", 		new LanguageCodeDef(){PluralRule=1, Code="it-IT", GoogleCode="it"}},
			{"Italian/Switzerland", new LanguageCodeDef(){PluralRule=1, Code="it-CH", GoogleCode="it"}},
			{"Japanese", 			new LanguageCodeDef(){PluralRule=0, Code="ja", HasJoinedWords=true}},
			{"Kannada", 			new LanguageCodeDef(){PluralRule=1, Code="kn"}},
			{"Kazakh", 				new LanguageCodeDef(){PluralRule=1, Code="kk"}},
			//{"Konkani", new LanguageCodeDef(){Code="koK"}},//----------------
			{"Korean", 				new LanguageCodeDef(){PluralRule=0, Code="ko"}},
			{"Kurdish", 			new LanguageCodeDef(){PluralRule=1, Code="ku"}},
			{"Kyrgyz", 				new LanguageCodeDef(){PluralRule=1, Code="ky"}},
			{"Latin", 				new LanguageCodeDef(){PluralRule=1, Code="la"}},
			{"Latvian", 			new LanguageCodeDef(){PluralRule=5, Code="lv"}},
			{"Lithuanian", 			new LanguageCodeDef(){PluralRule=5, Code="lt"}},
			{"Macedonian", 			new LanguageCodeDef(){PluralRule=13, Code="mk"}},
			{"Malay", 				new LanguageCodeDef(){PluralRule=0, Code="ms"}},
			{"Malay/Brunei Darussalam", new LanguageCodeDef(){PluralRule=0, Code="ms-BN", GoogleCode="ms"}},
			{"Malay/Malaysia", 		new LanguageCodeDef(){PluralRule=0, Code="ms-MY", GoogleCode="ms"}},
			{"Malayalam", 			new LanguageCodeDef(){PluralRule=1, Code="ml"}},
			{"Maltese", 			new LanguageCodeDef(){PluralRule=12, Code="mt"}},
			{"Maori", 				new LanguageCodeDef(){PluralRule=2, Code="mi"}},
			{"Marathi", 			new LanguageCodeDef(){PluralRule=1, Code="mr"}},
			{"Mongolian", 			new LanguageCodeDef(){PluralRule=1, Code="mn"}},
			{"Northern Sotho", 		new LanguageCodeDef(){PluralRule=1, Code="ns", GoogleCode="st"}},
			{"Norwegian", 			new LanguageCodeDef(){PluralRule=1, Code="nb", GoogleCode="no"}},
			{"Norwegian/Nynorsk", 	new LanguageCodeDef(){PluralRule=1, Code="nn", GoogleCode="no"}},
			{"Pashto", 				new LanguageCodeDef(){PluralRule=1, Code="ps"}},
			{"Persian", 			new LanguageCodeDef(){PluralRule=0, Code="fa"}},
			{"Polish", 				new LanguageCodeDef(){PluralRule=8, Code="pl"}},
			{"Portuguese", 			new LanguageCodeDef(){PluralRule=1, Code="pt"}},
			{"Portuguese/Brazil", 	new LanguageCodeDef(){PluralRule=2, Code="pt-BR", GoogleCode="pt"}},
			{"Portuguese/Portugal", new LanguageCodeDef(){PluralRule=1, Code="pt-PT", GoogleCode="pt"}},
			{"Punjabi", 			new LanguageCodeDef(){PluralRule=1, Code="pa"}},
			{"Quechua", 			new LanguageCodeDef(){PluralRule=1, Code="qu", GoogleCode="-"}},
			{"Quechua/Bolivia", 	new LanguageCodeDef(){PluralRule=1, Code="qu-BO", GoogleCode="-"}},
			{"Quechua/Ecuador", 	new LanguageCodeDef(){PluralRule=1, Code="qu-EC", GoogleCode="-"}},
			{"Quechua/Peru", 		new LanguageCodeDef(){PluralRule=1, Code="qu-PE", GoogleCode="-"}},
			{"Rhaeto-Romanic", 		new LanguageCodeDef(){PluralRule=1, Code="rm", GoogleCode="ro"}},
			{"Romanian", 			new LanguageCodeDef(){PluralRule=4, Code="ro"}},
			{"Russian", 			new LanguageCodeDef(){PluralRule=5, Code="ru"}},
			{"Russian/Republic of Moldova", new LanguageCodeDef(){PluralRule=5, Code="ru-MO", GoogleCode="ru"}},
			//{"Sami/Finland", new LanguageCodeDef(){Code="se-FI"}}, //--------------
			//{"Sami/Lappish", new LanguageCodeDef(){Code="sz"}}, //--------------
			//{"Sami/Northern", new LanguageCodeDef(){Code="se-NO"}}, //--------------
			//{"Sami/Sweden", new LanguageCodeDef(){Code="se-SE"}}, //--------------
			//{"Sanskrit", new LanguageCodeDef(){Code="sa"}}, //--------------
			{"Serbian", 			new LanguageCodeDef(){PluralRule=5, Code="sr"}},
			{"Serbian/Bosnia and Herzegovina", 	new LanguageCodeDef(){PluralRule=5, Code="sr-BA", GoogleCode="sr"}},
			{"Serbian/Serbia and Montenegro", 	new LanguageCodeDef(){PluralRule=5, Code="sr-SP", GoogleCode="sr"}},
			{"Slovak", 				new LanguageCodeDef(){PluralRule=7, Code="sk"}},
			{"Slovenian", 			new LanguageCodeDef(){PluralRule=9, Code="sl"}},
			//{"Sorbian", new LanguageCodeDef(){Code="sb"}}, //------------------------
			{"Spanish", 			new LanguageCodeDef(){PluralRule=1, Code="es"}},
			{"Spanish/Argentina", 	new LanguageCodeDef(){PluralRule=1, Code="es-AR", GoogleCode="es"}},
			{"Spanish/Bolivia", 	new LanguageCodeDef(){PluralRule=1, Code="es-BO", GoogleCode="es"}},
			{"Spanish/Castilian", 	new LanguageCodeDef(){PluralRule=1, Code="es-ES", GoogleCode="es"}},
			{"Spanish/Chile", 		new LanguageCodeDef(){PluralRule=1, Code="es-CL", GoogleCode="es"}},
			{"Spanish/Colombia", 	new LanguageCodeDef(){PluralRule=1, Code="es-CO", GoogleCode="es"}},
			{"Spanish/Costa Rica", 	new LanguageCodeDef(){PluralRule=1, Code="es-CR", GoogleCode="es"}},
			{"Spanish/Dominican Republic", new LanguageCodeDef(){PluralRule=1, Code="es-DO", GoogleCode="es"}},
			{"Spanish/Ecuador", 	new LanguageCodeDef(){PluralRule=1, Code="es-EC", GoogleCode="es"}},
			{"Spanish/El Salvador", new LanguageCodeDef(){PluralRule=1, Code="es-SV", GoogleCode="es"}},
			{"Spanish/Guatemala", 	new LanguageCodeDef(){PluralRule=1, Code="es-GT", GoogleCode="es"}},
			{"Spanish/Honduras", 	new LanguageCodeDef(){PluralRule=1, Code="es-HN", GoogleCode="es"}},
			{"Spanish/Mexico", 		new LanguageCodeDef(){PluralRule=1, Code="es-MX", GoogleCode="es"}},
			{"Spanish/Nicaragua", 	new LanguageCodeDef(){PluralRule=1, Code="es-NI", GoogleCode="es"}},
			{"Spanish/Panama", 		new LanguageCodeDef(){PluralRule=1, Code="es-PA", GoogleCode="es"}},
			{"Spanish/Paraguay", 	new LanguageCodeDef(){PluralRule=1, Code="es-PY", GoogleCode="es"}},
			{"Spanish/Peru", 		new LanguageCodeDef(){PluralRule=1, Code="es-PE", GoogleCode="es"}},
			{"Spanish/Puerto Rico", new LanguageCodeDef(){PluralRule=1, Code="es-PR", GoogleCode="es"}},
			{"Spanish/Spain", 		new LanguageCodeDef(){PluralRule=1, Code="es"}},
			{"Spanish/Uruguay", 	new LanguageCodeDef(){PluralRule=1, Code="es-UY", GoogleCode="es"}},
			{"Spanish/Venezuela", 	new LanguageCodeDef(){PluralRule=1, Code="es-VE", GoogleCode="es"}},
			//{"Sutu", new LanguageCodeDef(){Code="sx"}},//---------------
			{"Swahili", 			new LanguageCodeDef(){Code="sw"}},
			{"Swedish",				new LanguageCodeDef(){PluralRule=1, Code="sv"}},
			{"Swedish/Finland", 	new LanguageCodeDef(){PluralRule=1, Code="sv-FI", GoogleCode="sv"}},
			{"Swedish/Sweden", 		new LanguageCodeDef(){PluralRule=1, Code="sv-SE", GoogleCode="sv"}},
			//{"Syriac", new LanguageCodeDef(){Code="syR"}},//-----------
			{"Tamil", 				new LanguageCodeDef(){PluralRule=1, Code="ta"}},
			{"Tatar", 				new LanguageCodeDef(){PluralRule=0, Code="tt", GoogleCode="-"}},
			{"Telugu", 				new LanguageCodeDef(){PluralRule=1, Code="te"}},
			{"Thai", 				new LanguageCodeDef(){PluralRule=0, Code="th", HasJoinedWords=true}},
			//{"Tsonga", new LanguageCodeDef(){Code="ts"}},//-----------
			//{"Tswana", new LanguageCodeDef(){Code="tn"}},//-----------
			{"Turkish", 			new LanguageCodeDef(){PluralRule=0, Code="tr"}},
			{"Ukrainian", 			new LanguageCodeDef(){PluralRule=5, Code="uk"}},
			{"Urdu", 				new LanguageCodeDef(){PluralRule=1, Code="ur"}},
			{"Uzbek", 				new LanguageCodeDef(){PluralRule=2, Code="uz"}},
			//{"Venda", new LanguageCodeDef(){Code="ve"}},//------------
			{"Vietnamese", 			new LanguageCodeDef(){PluralRule=1, Code="vi"}},
			{"Welsh", 				new LanguageCodeDef(){PluralRule=16, Code="cy"}},
			{"Xhosa", 				new LanguageCodeDef(){PluralRule=1, Code="xh"}},
			{"Yiddish", 			new LanguageCodeDef(){PluralRule=1, Code="yi"}},
			{"Zulu", 				new LanguageCodeDef(){PluralRule=1, Code="zu"}}
		};

		static int GetPluralRule( string langCode )
		{
			if (langCode.Length > 2)
				langCode = langCode.Substring(0, 2);
			langCode = langCode.ToLower();

			foreach (var kvp in mLanguageDef)
				if (kvp.Value.Code == langCode) 
				{
					return kvp.Value.PluralRule;
				}
			return 0;			
		}


		//http://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html
		//http://cldr.unicode.org/cldr-features#TOC-Locale-specific-patterns-for-formatting-and-parsing:
		//http://cldr.unicode.org/index/cldr-spec/plural-rules
		public static bool LanguageHasPluralType( string langCode, string pluralType )
		{
            if (pluralType == "Plural" || pluralType=="Zero" || pluralType=="One")
                return true;

			int rule = GetPluralRule (langCode);

			switch (rule) 
			{
				case 3:	 	// Celtic (Scottish Gaelic)
							return 	pluralType=="Two" || pluralType=="Few";

				case 4:		// Families: Romanic (Romanian)
				case 5:		// Families: Baltic (Latvian, Lithuanian)
				case 6: 	// Families: Slavic (Belarusian, Bosnian, Croatian, Serbian, Russian, Ukrainian)
				case 7: 	// Families: Slavic (Slovak, Czech)
				case 8:		// Families: Slavic (Polish)
					return 	pluralType=="Few";

				case 9:	// Families: Slavic (Slovenian, Sorbian)
					return 	pluralType=="Two" || pluralType=="Few";

				case 10:	// Families: Celtic (Irish Gaelic)
				case 11: 	// Families: Semitic (Arabic)
				case 15: 	// Families: Celtic (Breton)
				case 16: 	// Families: (Welsh)
					return 	pluralType=="Two" || pluralType=="Few" || pluralType=="Many";

				case 12: // Families: Semitic (Maltese)
					return 	pluralType=="Few" || pluralType=="Many";

				case 13: // Families: Slavic (Macedonian)
					return 	pluralType=="Two";
			}

            return false;
		}

		// https://developer.mozilla.org/en-US/docs/Mozilla/Localization/Localization_and_Plurals
		public static ePluralType GetPluralType( string langCode, int n )
		{
			if (n == 0) return ePluralType.Zero;
			if (n == 1) return ePluralType.One;

			int rule = GetPluralRule (langCode);

			switch (rule) 
			{
				case 0: 	// Families: Asian (Chinese, Japanese, Korean), Persian, Turkic/Altaic (Turkish), Thai, Lao
						 	return ePluralType.Plural;

				case 1:  	// Families: Germanic (Danish, Dutch, English, Faroese, Frisian, German, Norwegian, Swedish), Finno-Ugric (Estonian, Finnish, Hungarian), Language isolate (Basque), Latin/Greek (Greek), Semitic (Hebrew), Romanic (Italian, Portuguese, Spanish, Catalan), Vietnamese
						 	return (n==1) ? ePluralType.One : ePluralType.Plural;

				case 2:	 	// Families: Romanic (French, Brazilian Portuguese)
						 	return (n<=1) ? ePluralType.One : ePluralType.Plural;

				case 3:	 	// Celtic (Scottish Gaelic)
							return 	(n==1 || n==11) ? ePluralType.One : 
									(n==2 || n==12) ? ePluralType.Two : 
									(inRange(n,3,10) || inRange(n,13,19)) ? ePluralType.Few : ePluralType.Plural;

				case 4:		// Families: Romanic (Romanian)
							return 	(n==1) ? ePluralType.One : 
									inRange(n%100, 1, 19) ? ePluralType.Few : ePluralType.Plural;

				case 5:		// Families: Baltic (Latvian, Lithuanian)
							return 	(n%10==1 && n%100!=11) ? ePluralType.One : 
									(n%10>=2 && (n%100<10 || n%100>=20)) ? ePluralType.Few : ePluralType.Plural;

				case 6: 	// Families: Slavic (Belarusian, Bosnian, Croatian, Serbian, Russian, Ukrainian)
							return 	(n % 10 == 1 && n % 100 != 11) ? ePluralType.One : 
									(inRange (n%10,2,4) && !inRange (n%100,12,14)) ? ePluralType.Few : ePluralType.Plural;

				case 7: 	// Families: Slavic (Slovak, Czech)
							return 	(n==1) ? ePluralType.One : 
									inRange(n,2,4) ? ePluralType.Few : ePluralType.Plural;

				case 8:		// Families: Slavic (Polish)
							return 	(n==1) ? ePluralType.One : 
									(inRange (n%10,2,4) && !inRange (n%100,12,14)) ? ePluralType.Few : ePluralType.Plural;

				case 9:	// Families: Slavic (Slovenian, Sorbian)
							return 	(n%100==1) ? ePluralType.One : 
									(n%100==2) ? ePluralType.Two : 
									inRange(n%100,3,4) ? ePluralType.Few : ePluralType.Plural;

				case 10:	// Families: Celtic (Irish Gaelic)
							return 	(n==1) ? ePluralType.One : 
									(n==2) ? ePluralType.Two : 
									inRange(n, 3,6) ? ePluralType.Few :
									inRange(n, 7,10)? ePluralType.Many : ePluralType.Plural;

				case 11: 	// Families: Semitic (Arabic)
							return 	(n==0) ? ePluralType.Zero : 
									(n==1) ? ePluralType.One : 
									(n==2) ? ePluralType.Two : 
									inRange(n%100,3,10) ? ePluralType.Few : 
									(n%100>=11) ? ePluralType.Many : ePluralType.Plural;

				case 12: // Families: Semitic (Maltese)
						return 	(n==1) ? ePluralType.One : 
								inRange(n%100, 1, 10) ? ePluralType.Few : 
								inRange(n%100, 11,19) ? ePluralType.Many : ePluralType.Plural;

				case 13: // Families: Slavic (Macedonian)
						return 	(n % 10 == 1) ? ePluralType.One :
								(n % 10 == 2) ? ePluralType.Two : ePluralType.Plural;

				case 14: // Plural rule #15 (2 forms)
						return 	(n%10==1 && n%100!=11) ? ePluralType.One : ePluralType.Plural;

				case 15: // Families: Celtic (Breton)
						return 	(n % 10 == 1 && (n % 100 != 11 && n % 100 != 71 && n % 100 != 91)) ? ePluralType.One : 
								(n % 10 == 2 && (n % 100 != 12 && n % 100 != 72 && n % 100 != 92)) ? ePluralType.Two : 
								((n % 10 == 3 || n % 10 == 4 || n % 10 == 9) && (n % 100 != 13 && n % 100 != 14 && n % 100 != 19 && n % 100 != 73 && n % 100 != 74 && n % 100 != 79 && n % 100 != 93 && n % 100 != 94 && n % 100 != 99)) ? ePluralType.Few : 
								(n%1000000==0) ? ePluralType.Many : ePluralType.Plural;

				case 16: // Families: (Welsh)
						return 	(n==0) ? ePluralType.Zero : 
								(n==1) ? ePluralType.One : 
								(n==2) ? ePluralType.Two : 
								(n==3) ? ePluralType.Few : 
								(n==6) ? ePluralType.Many : ePluralType.Plural;

			}

			return ePluralType.Plural;
		}

		// A number that belong to the pluralType form
		public static int GetPluralTestNumber( string langCode, ePluralType pluralType )
		{
			switch (pluralType) 
			{
				case ePluralType.Zero:
					return 0;

				case ePluralType.One:
					return 1;

				case ePluralType.Few:
					return 3;

				case ePluralType.Many:
				{
					int rule = GetPluralRule (langCode);
					if (rule == 10) return 8;
					if (rule == 11 || rule==12) return 13;
					if (rule == 15) return 1000000;
					return 6;
				}

				default:
					return 936;
			}
		}

		static bool inRange(int amount, int min, int max)
		{
			return amount >= min && amount <= max;
		}
	}
}