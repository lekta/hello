namespace LH.Imprint {
    public static class DebugInfoProvider {
        public static string GetImprintName(int imprintId) {
            var config = ResolveImprintsConfig();
            if (config == null) return imprintId.ToString();

            var imprint = config.GetImprint(imprintId);
            return imprint != null ? imprint.name : imprintId.ToString();
        }

        private static ImprintsConfig ResolveImprintsConfig() {
            if (RootConfigs.Instance != null)
                return RootConfigs.Instance.Imprints;

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<ImprintsConfig>(ImprintPaths.IMPRINTS_CONFIG);
#else
            return null;
#endif
        }
    }
}
