using System;
using Object = UnityEngine.Object;

namespace LH.Dev {
    public enum IssueSeverity { Warning, Error }

    public class ConfigIssue {
        public string Message;
        public IssueSeverity Severity;
        public Object Asset;
        public string PropertyPath;
        public Action AutoFix;

        public bool HasAutoFix => AutoFix != null;
    }
}
