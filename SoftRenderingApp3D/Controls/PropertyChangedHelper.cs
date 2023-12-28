namespace SoftRenderingApp3D.Controls {
    public static class PropertyChangedHelper {
        //public static bool ChangeValue<T>(ref T oldValue, T newValue) {
        //    if(Equals(oldValue, newValue)) {
        //        return false;
        //    }

        //    oldValue = newValue;
        //    return true;
        //}

        public static bool TryUpdateOther<T>(this T newValue , ref T oldValue) {
            if(Equals(oldValue, newValue)) 
                return false;

            oldValue = newValue;
            return true;
        }
    }
}