namespace SoftRenderingApp3D.Controls
{
    public static class PropertyChangedHelper
    {
        public static bool TryUpdateOther<T>(this T newValue, ref T oldValue)
        {
            if(Equals(oldValue, newValue))
                return false;

            oldValue = newValue;
            return true;
        }
    }
}
