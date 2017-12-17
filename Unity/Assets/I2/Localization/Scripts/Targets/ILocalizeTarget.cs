using System.Collections.Generic;
using UnityEngine;


namespace I2.Loc
{
    public abstract class ILocalizeTarget
    {
        public abstract bool FindTarget(Localize cmp);
        public abstract void GetFinalTerms( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm);
        public abstract void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation);

        public abstract ILocalizeTarget Clone(Localize cmp);
        public abstract string GetName();
        public abstract bool CanLocalize( Localize cmp );
        public abstract bool CanUseSecondaryTerm();
        public abstract bool AllowMainTermToBeRTL();
        public abstract bool AllowSecondTermToBeRTL();
        public abstract bool HasTarget(Localize cmp);
        public abstract eTermType GetPrimaryTermType(Localize cmp);
        public abstract eTermType GetSecondaryTermType(Localize cmp);
    }

    public abstract class LocalizeTarget<T> : ILocalizeTarget where T : Object
    {
        public override bool CanLocalize(Localize cmp)
        {
            return cmp.GetComponent<T>() != null;
        }

        public override bool FindTarget(Localize cmp)
        {
            cmp.mTarget = (cmp.mTarget as T) ?? cmp.GetComponent<T>();
            return (cmp.mTarget != null);
        }

        public T GetTarget( Localize cmp )
        {
            return cmp.mTarget as T;
        }

        public override bool HasTarget( Localize cmp)
        {
            return GetTarget(cmp) != null;
        }

        public override ILocalizeTarget Clone(Localize cmp)
        {
            return this.MemberwiseClone() as ILocalizeTarget;
        }


        public static H FindInParents<H> ( Transform tr ) where H : Component
		{
			if (!tr)
				return null;

			H comp = tr.GetComponent<H>();
			while (!comp && tr)
			{
				comp = tr.GetComponent<H>();
				tr = tr.parent;
			}
			return comp;
		}
	}

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class I2RuntimeInitialize : RuntimeInitializeOnLoadMethodAttribute
    {
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
        public I2RuntimeInitialize() : base(RuntimeInitializeLoadType.BeforeSceneLoad)
        {
        }
#else
        public I2RuntimeInitialize()
        {
        }
#endif
    }

}

