using System.Reflection;
using UnityEngine;

namespace Convergence
{
    /// <summary>
    /// Miembros de Unity que la 1.7 (motor Waylith, capa UnityStubs) ya no tiene. Se llaman por reflexión: en Waylith un miembro
    /// de Unity inexistente rompe el JIT de todo el método que lo nombra (MissingMethodException en cada cuadro), aunque esa línea
    /// no se ejecute. En 1.6 hacen exactamente lo mismo que antes; en 1.7 no hacen nada.
    /// </summary>
    public static class UnityCompat
    {
        private static readonly PropertyInfo captureFramerate = typeof(Time).GetProperty("captureFramerate", BindingFlags.Public | BindingFlags.Static);

        /// <summary><c>Time.captureFramerate</c> (grabación cuadro a cuadro para los GIF del Workshop). No-op en 1.7.</summary>
        public static int CaptureFramerate
        {
            set { if (captureFramerate != null && captureFramerate.CanWrite) captureFramerate.SetValue(null, value, null); }
        }
    }
}
