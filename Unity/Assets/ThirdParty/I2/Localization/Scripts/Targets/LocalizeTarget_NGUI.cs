#if NGUI

using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoad]
#endif

	public class LocalizeTarget_NGUI_Image : LocalizeTarget<UISprite>
	{
        static LocalizeTarget_NGUI_Image() { AutoRegister(); }
        [I2RuntimeInitialize] static void AutoRegister()
        {
            LocalizationManager.RegisterTarget(new LocalizeTarget_NGUI_Image());
        }

		public override string GetName () { return "NGUI UISprite"; }
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Sprite; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.UIAtlas; }
        public override bool CanUseSecondaryTerm () { return true; }
		public override bool AllowMainTermToBeRTL () { return false; }
		public override bool AllowSecondTermToBeRTL () { return false; }

		public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm )
		{
            var mTarget = GetTarget(cmp);
			primaryTerm = mTarget.spriteName;
			secondaryTerm = (mTarget.atlas != null ? mTarget.atlas.name : string.Empty);
        }


        public override void DoLocalize ( Localize cmp, string mainTranslation, string secondaryTranslation )
		{
            var mTarget = GetTarget(cmp);
            if (mTarget.spriteName == mainTranslation)
                return;

            //--[ Localize Atlas ]----------
            UIAtlas newAtlas = cmp.GetSecondaryTranslatedObj<UIAtlas>(ref mainTranslation, ref secondaryTranslation);
            bool bChanged = false;
            if (newAtlas != null && mTarget.atlas != newAtlas)
            {
                mTarget.atlas = newAtlas;
                bChanged = true;
            }

            if (mTarget.spriteName != mainTranslation && mTarget.atlas.GetSprite(mainTranslation) != null)
            {
                mTarget.spriteName = mainTranslation;
                bChanged = true;
            }
            if (bChanged)
                mTarget.MakePixelPerfect();
        }
	}

    #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoad]
#endif

    public class LocalizeTarget_NGUI_Texture : LocalizeTarget<UITexture>
    {
        static LocalizeTarget_NGUI_Texture() { AutoRegister(); }
        [I2RuntimeInitialize]
        static void AutoRegister()
        {
            LocalizationManager.RegisterTarget(new LocalizeTarget_NGUI_Texture());
        }
        public override string GetName() { return "NGUI UITexture"; }
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Texture; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; }
        public override bool CanUseSecondaryTerm() { return false; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            var mTarget = GetTarget(cmp);
            primaryTerm = mTarget.mainTexture.name;
            secondaryTerm = null;
        }

        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var mTarget = GetTarget(cmp);
            Texture Old = mTarget.mainTexture;
            if (Old != null && Old.name != mainTranslation)
            {
                mTarget.mainTexture = cmp.FindTranslatedObject<Texture>(mainTranslation);
                mTarget.MakePixelPerfect();
            }
        }
    }

    #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoad]
#endif

    public class LocalizeTarget_NGUI_Label : LocalizeTarget<UILabel>
    {
        static LocalizeTarget_NGUI_Label() { AutoRegister(); }
        [I2RuntimeInitialize] static void AutoRegister()
        {
            LocalizationManager.RegisterTarget(new LocalizeTarget_NGUI_Label());
        }
        public NGUIText.Alignment mAlignment_RTL = NGUIText.Alignment.Right;
        public NGUIText.Alignment mAlignment_LTR = NGUIText.Alignment.Left;
        public bool mAlignmentWasRTL;
        public bool mInitializeAlignment = true;

        public override string GetName() { return "UILabel"; }
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.UIFont; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            var mTarget = GetTarget(cmp);
            primaryTerm = mTarget.text;
            secondaryTerm = (mTarget.ambigiousFont != null ? mTarget.ambigiousFont.name : string.Empty); ;
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var mTarget = GetTarget(cmp);

            //--[ Localize Font Object ]----------
            Font newFont = cmp.GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
            if (newFont != null)
            {
                if (newFont != mTarget.ambigiousFont)
                    mTarget.ambigiousFont = newFont;
            }
            else
            {
                UIFont newUIFont = cmp.GetSecondaryTranslatedObj<UIFont>(ref mainTranslation, ref secondaryTranslation);
                if (newUIFont != null && mTarget.ambigiousFont != newUIFont)
                    mTarget.ambigiousFont = newUIFont;
            }

            if (mInitializeAlignment)
            {
                mInitializeAlignment = false;
                mAlignment_LTR = mAlignment_RTL = mTarget.alignment;

                if (LocalizationManager.IsRight2Left && mAlignment_RTL == NGUIText.Alignment.Right)
                    mAlignment_LTR = NGUIText.Alignment.Left;
                if (!LocalizationManager.IsRight2Left && mAlignment_LTR == NGUIText.Alignment.Left)
                    mAlignment_RTL = NGUIText.Alignment.Right;
            }

            UIInput input = NGUITools.FindInParents<UIInput>(mTarget.gameObject);
            if (input != null && input.label == mTarget)
            {
                if (mainTranslation != null && input.defaultText != mainTranslation)
                {
                    if (cmp.CorrectAlignmentForRTL && (input.label.alignment == NGUIText.Alignment.Left || input.label.alignment == NGUIText.Alignment.Right))
                        input.label.alignment = (LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR);

                    input.defaultText = mainTranslation;
                }
            }
            else
            {
                if (mainTranslation != null && mTarget.text != mainTranslation)
                {
                    if (cmp.CorrectAlignmentForRTL && (mTarget.alignment == NGUIText.Alignment.Left || mTarget.alignment == NGUIText.Alignment.Right))
                        mTarget.alignment = (LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR);

                    mTarget.text = mainTranslation;
                }
            }
        }
    }
}
#endif

