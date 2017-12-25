using System;
using UnityEngine;

#if TextMeshPro
namespace I2.Loc
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif

    public class LocalizeTarget_TextMeshPro_TMPLabel : LocalizeTarget<TMPro.TextMeshPro>
    {
        static LocalizeTarget_TextMeshPro_TMPLabel() { AutoRegister(); }
        [I2RuntimeInitialize]
        static void AutoRegister()
        {
            LocalizationManager.RegisterTarget(new LocalizeTarget_TextMeshPro_TMPLabel());
        }
        public TMPro.TextAlignmentOptions mAlignment_RTL = TMPro.TextAlignmentOptions.Right;
        public TMPro.TextAlignmentOptions mAlignment_LTR = TMPro.TextAlignmentOptions.Left;
        public bool mAlignmentWasRTL;
        public bool mInitializeAlignment = true;

        public override string GetName() { return "TextMeshPro Label"; }
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Font; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            var mTarget = GetTarget(cmp);
            primaryTerm = mTarget.text;
            secondaryTerm = (mTarget.font != null ? mTarget.font.name : string.Empty);
        }

        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var mTarget = GetTarget(cmp);
            //--[ Localize Font Object ]----------
            {
                TMPro.TMP_FontAsset newFont = cmp.GetSecondaryTranslatedObj<TMPro.TMP_FontAsset>(ref mainTranslation, ref secondaryTranslation);

                if (newFont != null)
                {
                    if (mTarget.font != newFont)
                        mTarget.font = newFont;
                }
                else
                {
                    //--[ Localize Font Material ]----------
                    Material newMat = cmp.GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
                    if (newMat != null && mTarget.fontMaterial != newMat)
                    {
                        if (!newMat.name.StartsWith(mTarget.font.name, StringComparison.Ordinal))
                        {
                            newFont = GetTMPFontFromMaterial(cmp, secondaryTranslation.EndsWith(newMat.name, StringComparison.Ordinal) ? secondaryTranslation : newMat.name);
                            if (newFont != null)
                                mTarget.font = newFont;
                        }

                        mTarget.fontSharedMaterial/* fontMaterial*/ = newMat;
                    }
                }
            }
            if (mInitializeAlignment)
            {
                mInitializeAlignment = false;
                mAlignmentWasRTL = LocalizationManager.IsRight2Left;
                InitAlignment_TMPro(mAlignmentWasRTL, mTarget.alignment, out mAlignment_LTR, out mAlignment_RTL);
            }
            else
            {
                TMPro.TextAlignmentOptions alignRTL, alignLTR;
                InitAlignment_TMPro(mAlignmentWasRTL, mTarget.alignment, out alignLTR, out alignRTL);

                if ((mAlignmentWasRTL && mAlignment_RTL != alignRTL) ||
                    (!mAlignmentWasRTL && mAlignment_LTR != alignLTR))
                {
                    mAlignment_LTR = alignLTR;
                    mAlignment_RTL = alignRTL;
                }
                mAlignmentWasRTL = LocalizationManager.IsRight2Left;
            }

            if (mainTranslation != null && mTarget.text != mainTranslation)
            {
                if (cmp.CorrectAlignmentForRTL)
                {
                    mTarget.alignment = (LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR);
                    mTarget.isRightToLeftText = LocalizationManager.IsRight2Left;
                    if (LocalizationManager.IsRight2Left) mainTranslation = LocalizationManager.ReverseText(mainTranslation);
                }

                mTarget.text = mainTranslation;
            }
        }

#region Tools
        internal static TMPro.TMP_FontAsset GetTMPFontFromMaterial(Localize cmp, string matName)
        {
            string splitChars = " .\\/-[]()";
            for (int i = matName.Length - 1; i > 0;)
            {
                // Find first valid character
                while (i > 0 && splitChars.IndexOf(matName[i]) >= 0)
                    i--;

                if (i <= 0) break;

                var fontName = matName.Substring(0, i + 1);
                var obj = cmp.GetObject<TMPro.TMP_FontAsset>(fontName);
                if (obj != null)
                    return obj;

                // skip this word
                while (i > 0 && splitChars.IndexOf(matName[i]) < 0)
                    i--;
            }

            return null;
        }

        internal static void InitAlignment_TMPro(bool isRTL, TMPro.TextAlignmentOptions alignment, out TMPro.TextAlignmentOptions alignLTR, out TMPro.TextAlignmentOptions alignRTL)
        {
            alignLTR = alignRTL = alignment;

            if (isRTL)
            {
                switch (alignment)
                {
                    case TMPro.TextAlignmentOptions.TopRight: alignLTR = TMPro.TextAlignmentOptions.TopLeft; break;
                    case TMPro.TextAlignmentOptions.Right: alignLTR = TMPro.TextAlignmentOptions.Left; break;
                    case TMPro.TextAlignmentOptions.BottomRight: alignLTR = TMPro.TextAlignmentOptions.BottomLeft; break;
                    case TMPro.TextAlignmentOptions.BaselineRight: alignLTR = TMPro.TextAlignmentOptions.BaselineLeft; break;
                    case TMPro.TextAlignmentOptions.MidlineRight: alignLTR = TMPro.TextAlignmentOptions.MidlineLeft; break;
                    case TMPro.TextAlignmentOptions.CaplineRight: alignLTR = TMPro.TextAlignmentOptions.CaplineLeft; break;

                    case TMPro.TextAlignmentOptions.TopLeft: alignLTR = TMPro.TextAlignmentOptions.TopRight; break;
                    case TMPro.TextAlignmentOptions.Left: alignLTR = TMPro.TextAlignmentOptions.Right; break;
                    case TMPro.TextAlignmentOptions.BottomLeft: alignLTR = TMPro.TextAlignmentOptions.BottomRight; break;
                    case TMPro.TextAlignmentOptions.BaselineLeft: alignLTR = TMPro.TextAlignmentOptions.BaselineRight; break;
                    case TMPro.TextAlignmentOptions.MidlineLeft: alignLTR = TMPro.TextAlignmentOptions.MidlineRight; break;
                    case TMPro.TextAlignmentOptions.CaplineLeft: alignLTR = TMPro.TextAlignmentOptions.CaplineRight; break;

                }
            }
            else
            {
                switch (alignment)
                {
                    case TMPro.TextAlignmentOptions.TopRight: alignRTL = TMPro.TextAlignmentOptions.TopLeft; break;
                    case TMPro.TextAlignmentOptions.Right: alignRTL = TMPro.TextAlignmentOptions.Left; break;
                    case TMPro.TextAlignmentOptions.BottomRight: alignRTL = TMPro.TextAlignmentOptions.BottomLeft; break;
                    case TMPro.TextAlignmentOptions.BaselineRight: alignRTL = TMPro.TextAlignmentOptions.BaselineLeft; break;
                    case TMPro.TextAlignmentOptions.MidlineRight: alignRTL = TMPro.TextAlignmentOptions.MidlineLeft; break;
                    case TMPro.TextAlignmentOptions.CaplineRight: alignRTL = TMPro.TextAlignmentOptions.CaplineLeft; break;

                    case TMPro.TextAlignmentOptions.TopLeft: alignRTL = TMPro.TextAlignmentOptions.TopRight; break;
                    case TMPro.TextAlignmentOptions.Left: alignRTL = TMPro.TextAlignmentOptions.Right; break;
                    case TMPro.TextAlignmentOptions.BottomLeft: alignRTL = TMPro.TextAlignmentOptions.BottomRight; break;
                    case TMPro.TextAlignmentOptions.BaselineLeft: alignRTL = TMPro.TextAlignmentOptions.BaselineRight; break;
                    case TMPro.TextAlignmentOptions.MidlineLeft: alignRTL = TMPro.TextAlignmentOptions.MidlineRight; break;
                    case TMPro.TextAlignmentOptions.CaplineLeft: alignRTL = TMPro.TextAlignmentOptions.CaplineRight; break;
                }
            }
        }

        internal static string ReverseText(string source)
        {
            var len = source.Length;
            var output = new char[len];
            for (var i = 0; i < len; i++)
            {
                output[(len - 1) - i] = source[i];
            }
            return new string(output);
        }
#endregion
    }

    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif

    public class LocalizeTarget_TextMeshPro_UGUI : LocalizeTarget<TMPro.TextMeshProUGUI>
    {
        static LocalizeTarget_TextMeshPro_UGUI() { AutoRegister(); }
        [I2RuntimeInitialize]
        static void AutoRegister()
        {
            LocalizationManager.RegisterTarget(new LocalizeTarget_TextMeshPro_UGUI());
        }
        public TMPro.TextAlignmentOptions mAlignment_RTL = TMPro.TextAlignmentOptions.Right;
        public TMPro.TextAlignmentOptions mAlignment_LTR = TMPro.TextAlignmentOptions.Left;
        public bool mAlignmentWasRTL;
        public bool mInitializeAlignment = true;

        public override string GetName() { return "TextMeshPro UGUI"; }
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.TextMeshPFont; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            var mTarget = GetTarget(cmp);
            primaryTerm = mTarget.text;
            secondaryTerm = (mTarget.font != null ? mTarget.font.name : string.Empty);
        }

        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var mTarget = GetTarget(cmp);

            {
                //--[ Localize Font Object ]----------
                TMPro.TMP_FontAsset newFont = cmp.GetSecondaryTranslatedObj<TMPro.TMP_FontAsset>(ref mainTranslation, ref secondaryTranslation);

                if (newFont != null)
                {
                    if (mTarget.font != newFont)
                        mTarget.font = newFont;
                }
                else
                {
                    //--[ Localize Font Material ]----------
                    Material newMat = cmp.GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
                    if (newMat != null && mTarget.fontMaterial != newMat)
                    {
                        if (!newMat.name.StartsWith(mTarget.font.name, StringComparison.Ordinal))
                        {
                            newFont = LocalizeTarget_TextMeshPro_TMPLabel.GetTMPFontFromMaterial(cmp, secondaryTranslation.EndsWith(newMat.name, StringComparison.Ordinal) ? secondaryTranslation : newMat.name);
                            if (newFont != null)
                                mTarget.font = newFont;
                        }
                        mTarget.fontSharedMaterial = newMat;
                    }
                }
            }

            if (mInitializeAlignment)
            {
                mInitializeAlignment = false;
                mAlignmentWasRTL = LocalizationManager.IsRight2Left;
                LocalizeTarget_TextMeshPro_TMPLabel.InitAlignment_TMPro(mAlignmentWasRTL, mTarget.alignment, out mAlignment_LTR, out mAlignment_RTL);
            }
            else
            {
                TMPro.TextAlignmentOptions alignRTL, alignLTR;
                LocalizeTarget_TextMeshPro_TMPLabel.InitAlignment_TMPro(mAlignmentWasRTL, mTarget.alignment, out alignLTR, out alignRTL);

                if ((mAlignmentWasRTL && mAlignment_RTL != alignRTL) ||
                    (!mAlignmentWasRTL && mAlignment_LTR != alignLTR))
                {
                    mAlignment_LTR = alignLTR;
                    mAlignment_RTL = alignRTL;
                }
                mAlignmentWasRTL = LocalizationManager.IsRight2Left;
            }

            if (mainTranslation != null && mTarget.text != mainTranslation)
            {
                if (cmp.CorrectAlignmentForRTL)
                {
                    mTarget.alignment = (LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR);
                    mTarget.isRightToLeftText = LocalizationManager.IsRight2Left;
                    if (LocalizationManager.IsRight2Left) mainTranslation = LocalizeTarget_TextMeshPro_TMPLabel.ReverseText(mainTranslation);
                }

                mTarget.text = mainTranslation;
            }
        }
    }
}
#endif