using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Alterful.GlobalHotKey
{
    /// <summary>
    /// 热键管理器
    /// </summary>
    class HotKeyManager
    {
        /// <summary>
        /// 热键消息
        /// </summary>
        public const int WM_HOTKEY = 0x312;

        /// <summary>
        /// 注册热键
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifuers, int vk);

        /// <summary>
        /// 注销热键
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// 向原子表中添加全局原子
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        /// <summary>
        /// 在表中搜索全局原子
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalFindAtom(string lpString);

        /// <summary>
        /// 在表中删除全局原子
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);
    }
}
