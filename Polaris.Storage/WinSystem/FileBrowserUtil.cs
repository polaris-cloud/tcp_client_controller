using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Polaris.Storage.WinSystem
{
    public class FileBrowserUtil
    {
        #region windows自带的文件管理器控件

        /// <summary>
        /// 创建打开文件管理器 ,默认json
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <param name="multiSelect"></param>
        /// <returns></returns>
        public static OpenFileDialog CreateOpenFileExplorer
            (string path, string filter = "Json文件|*.Json|所有文件|*.*", bool multiSelect = true)
        {

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "选择文件",
                InitialDirectory = path,
                Multiselect = multiSelect,
                Filter = filter,
                CheckPathExists = true,
                AddExtension = true
            };

            return dialog;

        }

        /// <summary>
        /// 创建保存文件管理器，默认json
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        public static SaveFileDialog CreateSaveFileExplorer
            (string path, string filter = "Json文件|*.Json|所有文件|*.*")
        {

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "保存文件",
                InitialDirectory = path,
                Filter = filter,
                CheckPathExists = true,
                AddExtension = true
            };
            return dialog;

        }

        /// <summary>
        ///  获取文件夹路径的dialog
        /// </summary>
        /// <param name="path"></param>
        /// <param name="multiSelect"></param>
        public static OpenFolderDialog CreateFolderExplorer
            (string path, bool multiSelect = true)
        {
            OpenFolderDialog openFolderDialog =
                new()
                {
                    Multiselect = true,
                    InitialDirectory = path
                };
            return openFolderDialog;
        }



        #endregion

    }
}
