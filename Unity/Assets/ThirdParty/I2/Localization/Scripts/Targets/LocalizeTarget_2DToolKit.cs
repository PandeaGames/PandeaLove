#if TK2D

using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoad]
#endif

    public class LocalizeTarget_2DToolKit_Sprite : LocalizeTarget<tk2dBaseSprite>
    {
        static LocalizeTarget_2DToolKit_Sprite() { AutoRegister(); }
        [I2RuntimeInitialize]
        static void AutoRegister()
        {
            LocalizationManager.RegisterTarget(new LocalizeTarget_2DToolKit_Sprite());
        }

        public override string GetName() { return "2DToolKit Sprite"; }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.TK2dCollection; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.TK2dCollection; }

        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            var mTarget = GetTarget(cmp);

            primaryTerm = (mTarget.CurrentSprite != null ? mTarget.CurrentSprite.name : string.Empty);
            secondaryTerm = (mTarget.Collection != null ? mTarget.Collection.spriteCollectionName : null);
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var mTarget = GetTarget(cmp);

            //--[ Localize Atlas ]----------
            tk2dSpriteCollection newCollection = cmp.GetSecondaryTranslatedObj<tk2dSpriteCollection>(ref mainTranslation, ref secondaryTranslation);

            if (newCollection != null)
            {
                if (mTarget.CurrentSprite.name != mainTranslation || mTarget.Collection.name != secondaryTranslation)
                    mTarget.SetSprite(newCollection.spriteCollection, mainTranslation);
            }
            else
            {
                if (mTarget.CurrentSprite.name != mainTranslation)
                    mTarget.SetSprite(mainTranslation);
            }
        }
    }

    #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoad]
#endif

    public class LocalizeTarget_2DToolKit_Label : LocalizeTarget<tk2dTextMesh>
    {
        static LocalizeTarget_2DToolKit_Label() { AutoRegister(); }
        [I2RuntimeInitialize]
        static void AutoRegister()
        {
            LocalizationManager.RegisterTarget(new LocalizeTarget_2DToolKit_Label());
        }
        TextAnchor mOriginalAlignment = TextAnchor.MiddleCenter;
        bool mInitializeAlignment = true;

        public override string GetName() { return "2DToolKit Label"; }
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.TK2dFont; }

        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            var mTarget = GetTarget(cmp);

            primaryTerm = mTarget.text;
            secondaryTerm = (mTarget.font != null ? mTarget.font.name : string.Empty);
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var mTarget = GetTarget(cmp);

            //--[ Localize Font Object ]----------
            tk2dFont newFont = cmp.GetSecondaryTranslatedObj<tk2dFont>(ref mainTranslation, ref secondaryTranslation);
            if (newFont != null && mTarget.font != newFont)
            {
                mTarget.font = newFont.data;
            }


            if (mInitializeAlignment)
            {
                mInitializeAlignment = false;
                mOriginalAlignment = mTarget.anchor;
            }

            if (mainTranslation != null && mTarget.text != mainTranslation)
            {
                if (Localize.CurrentLocalizeComponent.CorrectAlignmentForRTL)
                {
                    int align = (int)mTarget.anchor;

                    if (align % 3 == 0)
                        mTarget.anchor = LocalizationManager.IsRight2Left ? mTarget.anchor + 2 : mOriginalAlignment;
                    else
                    if (align % 3 == 2)
                        mTarget.anchor = LocalizationManager.IsRight2Left ? mTarget.anchor - 2 : mOriginalAlignment;
                }
                mTarget.text = mainTranslation;
            }
        }
    }
}
#endif

