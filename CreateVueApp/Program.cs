using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CreateVueApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CmdUtility cmdUtility = new CmdUtility();
            try
            {
                var isError = false;
                //执行node -v
                string nodeResult = cmdUtility.ExecuteInCMD("node -v", "");
                Console.WriteLine("node -v:" + nodeResult);
                ////执行vue -V
                //string vueResult = cmdUtility.ExecuteInCMD("vue -V", "");
                //Console.WriteLine("vue -V:" + vueResult);
                //执行yarn -v
                string yarnResult = cmdUtility.ExecuteInCMD("yarn -v","");
                Console.WriteLine("yarn -v:" + yarnResult);
                if (nodeResult == "bad" || yarnResult == "bad")
                {
                    isError = true;
                }

                if (isError)
                {
                    Console.WriteLine("----------------环境校验失败!--------------");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("----------------环境校验失败!--------------");
                throw e;
            }

            Console.WriteLine("----------------环境校验成功!--------------");

            var isWhile = true;
            string strInput = "";
            string strOutput = "";
            while (isWhile)
            {
                Console.Write("请输入前端工程目录地址:");
                strInput = Console.ReadLine();
                Console.Write("请输入打包成功需要保存的地址:");
                strOutput = Console.ReadLine();
                if (string.IsNullOrEmpty(strInput) || string.IsNullOrEmpty(strOutput))
                {
                    Console.WriteLine("必须输入指定的前端工目录地址和打包成功需要保存的地址!");
                    isWhile = true;
                }
                else
                {
                    isWhile = false;
                }
            }
            
            if (!string.IsNullOrWhiteSpace(strInput))
            {
                try
                {
                    string[] path = Directory.GetDirectories(@strInput);
                    string[] filePath = Directory.GetFiles(@strInput);
                    //如果扫描到的文件存在node_modules,则认为是单个项目(需要注意的是如果单个文件刚进行下载没有进行install的情况下是不存在node_modules文件夹的)
                    //针对以上情况则需要进一步判断是否是单个文件夹的情况
                    var singleProject = path.Where(file => file.Contains("node_modules")).Count();
                    bool singleFile = false;
                    if (singleProject == 0)
                    {
                        var singleNumber = filePath.Where(single => single.Contains("package.json") || single.Contains("babel.config.js")).Count();
                        if (singleNumber > 0)
                        {
                            singleFile = true;
                        }
                    }
                    else if(singleProject == 1)
                    {
                        //扫描到node_modules则认为直接是一个可直接进行打包的前端程序
                        singleFile = true;
                    }

                    if (singleFile)
                    {
                        //var singleFile = Directory.GetFiles(@strInput);
                        DirectoryInfo root = new DirectoryInfo(@strInput);
                        Dictionary<string, FileInfo[]> fileDictionary = new Dictionary<string, FileInfo[]>();
                        foreach (FileInfo file in root.GetFiles())
                        {
                            if (file.Name == "package.json")
                            {
                                fileDictionary.Add(file.DirectoryName, root.GetFiles());
                                break;
                            }
                        }
                        //执行打包程序
                        foreach (var builder in fileDictionary)
                        {
                            //判断那些程序使用yarn进行打包构建，那些使用npm进行打包构建
                            //1.执行node -v 判断是否有node运行环境
                            //2.执行vue -V 判断是否安装vue-cli
                            //3.执行npm install 或者 yarn install
                            //4.执行vue-cli-service build
                            var yarnBuilder = builder.Value.Where(item => item.Name == "yarn.lock").Count();
                            var targetFile = builder.Value.Select(s => s.DirectoryName).FirstOrDefault();
                            try
                            {
                                if (yarnBuilder != 0)
                                {
                                    //执行yarn install
                                    string yarnResult = cmdUtility.ExecuteInCMD("yarn install", targetFile);
                                    Console.WriteLine(yarnResult);
                                    //执行vue-cli-service build
                                    string cliResult = cmdUtility.ExecuteInCMD("yarn build", targetFile);
                                    Console.WriteLine(cliResult);
                                }
                                else
                                {
                                    //执行npm install
                                    string npmResult = cmdUtility.ExecuteInCMD("npm install", targetFile);
                                    Console.WriteLine(npmResult);
                                    //执行vue-cli-service build
                                    string cliResult = cmdUtility.ExecuteInCMD("npm run-script build", targetFile);
                                    Console.WriteLine(cliResult);
                                }
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }
                        }

                        //打包完毕之后判断是否存在vue-config.js文件，如果存在则读取对应的配置信息来确定那个文件是打包之后生成的文件
                        var isDefaultFile = filePath.Where(item => item.Contains("vue.config.js")).SingleOrDefault();
                        var targetPackage = "";
                        if (isDefaultFile != null)
                        {
                            var textLines = System.IO.File.ReadAllLines(isDefaultFile);
                            var builderPackage = "";
                            for (var i = 0; i < textLines.Length; i++)
                            {
                                if (textLines[i].Contains("outputDir"))
                                {
                                    builderPackage = textLines[i];
                                    break;
                                }
                            }
                            Regex re = new Regex("(?<=\").*?(?=\")", RegexOptions.None);
                            //如果配置文件没有指定打包目录则使用默认文件夹dist
                            if (!string.IsNullOrEmpty(builderPackage))
                            {
                                var substringPackage = builderPackage.Substring(builderPackage.IndexOf(":") + 1);
                                MatchCollection mc = re.Matches(substringPackage);
                                foreach (Match ma in mc)
                                {
                                    targetPackage = ma.ToString();
                                    break;
                                }
                            }
                            else
                            {
                                targetPackage = "dist";
                            }
                        }
                        else
                        {
                            targetPackage = "dist";
                        }

                        //移动文件夹
                        removeFile(targetPackage, strInput, @strOutput);
                        //都操作完毕之后自动打开对应的文件夹
                        System.Diagnostics.Process.Start("explorer.exe", strOutput);
                    }
                    else
                    {
                        //多前端项目打包操作
                        generateModule(path, @strOutput);
                    }
                    
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                
            }
        }
        /// <summary>
        /// 多文件打包程序
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void generateModule(string[] path,string newpath)
        {
            CmdUtility cmdUtility = new CmdUtility();

            Dictionary<string, List<FileInfo>> fileDictionary = new Dictionary<string, List<FileInfo>>();

            //获取指定文件夹下的所有文件
            for (var i = 0; i < path.Length; i++)
            {
                DirectoryInfo di = new DirectoryInfo(path[i]);
                //找到该目录下的文件
                FileInfo[] fi = di.GetFiles();
                //把FileInfo[]数组转换为List
                if (!fileDictionary.ContainsKey(path[i]))
                {
                    fileDictionary.Add(path[i], fi.ToList<FileInfo>());
                }
                else
                {
                    fileDictionary[path[i]].AddRange(fi.ToList<FileInfo>());
                }
            }
            List<string> waringFile = new List<string>();
            //将有问题的前端项目进行获取
            foreach (var dictionary in fileDictionary)
            {
                var dicList = dictionary.Value.Where(item => item.Name == "package.json" || item.Name == "babel.config.js");
                if (dicList.Count() == 0)
                {
                    waringFile.Add(dictionary.Key);
                    fileDictionary.Remove(dictionary.Key);
                }
            }
            //获取存在问题文件夹下的子文件夹进行重新扫描
            for (var i = 0; i < waringFile.Count; i++)
            {
                if (!waringFile[i].Contains("node_modules"))
                {
                    List<FileInfo> fileList = GetAllFilesByDir(waringFile[i]);
                    foreach (var file in fileList)
                    {
                        if (file.Name == "package.json")
                        {
                            var dictionaryName = file.DirectoryName;
                            var allFile = fileList.Where(item => item.DirectoryName == dictionaryName).ToList();
                            fileDictionary.Add(dictionaryName, allFile);
                        }
                    }
                }
            }
            //找到文件字典中key下所有的子级文件夹
            Dictionary<string, string[]> dictionaryStr = new Dictionary<string, string[]>();
            foreach (var dic in fileDictionary)
            {
                string[] dicList = Directory.GetDirectories(dic.Key);
                dictionaryStr.Add(dic.Key, dicList);
            }
            //执行打包程序
            foreach (var builder in fileDictionary)
            {
                //判断那些程序使用yarn进行打包构建，那些使用npm进行打包构建
                //1.执行node -v 判断是否有node运行环境
                //2.执行vue -V 判断是否安装vue-cli
                //3.执行npm install 或者 yarn install
                //4.执行vue-cli-service build
                var yarnBuilder = builder.Value.Where(item => item.Name == "yarn.lock").Count();
                var targetFile = builder.Value.Select(s => s.DirectoryName).FirstOrDefault();
                try
                {
                    if (yarnBuilder != 0)
                    {
                        //执行yarn install
                        string yarnResult = cmdUtility.ExecuteInCMD("yarn install", targetFile);
                        Console.WriteLine(yarnResult);
                        //执行vue-cli-service build
                        string cliResult = cmdUtility.ExecuteInCMD("yarn build", targetFile);
                        Console.WriteLine(cliResult);
                    }
                    else
                    {
                        //执行npm install
                        string npmResult = cmdUtility.ExecuteInCMD("npm install", targetFile);
                        Console.WriteLine(npmResult);
                        //执行vue-cli-service build
                        string cliResult = cmdUtility.ExecuteInCMD("npm run-script build", targetFile);
                        Console.WriteLine(cliResult);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                //打包完毕之后判断是否存在vue-config.js文件，如果存在则读取对应的配置信息来确定那个文件是打包之后生成的文件
                if (dictionaryStr.ContainsKey(builder.Key))
                {
                    string[] reloadDic = Directory.GetFiles(builder.Key);
                    var isDefaultFile = reloadDic.Where(item => item.Contains("vue.config.js")).SingleOrDefault();
                    var targetPackage = "";
                    if (isDefaultFile != null)
                    {
                        var textLines = System.IO.File.ReadAllLines(isDefaultFile);
                        var builderPackage = "";
                        for (var i = 0; i < textLines.Length; i++)
                        {
                            if (textLines[i].Contains("outputDir"))
                            {
                                builderPackage = textLines[i];
                                break;
                            }
                        }
                        Regex re = new Regex("(?<=\").*?(?=\")", RegexOptions.None);
                        //如果配置文件没有指定打包目录则使用默认文件夹dist
                        if (!string.IsNullOrEmpty(builderPackage))
                        {
                            var substringPackage = builderPackage.Substring(builderPackage.IndexOf(":") + 1);
                            MatchCollection mc = re.Matches(substringPackage);
                            foreach (Match ma in mc)
                            {
                                targetPackage = ma.ToString();
                                break;
                            }
                        }
                        else
                        {
                            targetPackage = "dist";
                        }
                    }
                    else
                    {
                        targetPackage = "dist";
                    }
                    //移动文件夹
                    removeFile(targetPackage, builder.Key, newpath);
                }
            }
            //都操作完毕之后自动打开对应的文件夹
            System.Diagnostics.Process.Start("explorer.exe", newpath);
        }

        /// <summary>
        /// 移动文件夹
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="oldpath"></param>
        /// <param name="newpath"></param>
        public static void removeFile(String filename, String oldpath, String newpath)
        {
            if (!oldpath.Equals(newpath))
            {
                var newFile = oldpath + "\\" + filename;
                DirectoryInfo oldfile = new DirectoryInfo(@newFile);
                var oldFile = newpath + "\\" + filename;
                DirectoryInfo newfile = new DirectoryInfo(@oldFile);
                if (oldfile.Exists)
                {
                    if (newfile.Exists)
                    {
                        Console.WriteLine("文件已存在，是否覆盖？1:是；2:不是。");
                        int key = Convert.ToInt32(Console.ReadLine());
                        if (key == 1)
                        {
                            Directory.Delete(@oldFile,true);
                            oldfile.MoveTo(newpath + "\\" + filename);
                            Console.WriteLine("文件移动成功,是否需要重命名该文件？1:是；2:不是。");
                            int rename = Convert.ToInt32(Console.ReadLine());
                            if (rename == 1)
                            {
                                Console.WriteLine("请输入新的文件名：");
                                String filenames = Console.ReadLine();
                                renameFile(newpath, filename, filenames);
                            }
                            else
                            {
                                Console.WriteLine("操作完成！");
                            }
                        }
                        else
                        {
                            Console.WriteLine("已取消移动！");
                        }
                    }
                    else
                    {
                        oldfile.MoveTo(newpath + "\\" + filename);
                        Console.WriteLine("文件移动成功,是否需要重命名该文件？1:是；2:不是。");
                        int rename = Convert.ToInt32(Console.ReadLine());
                        if (rename == 1)
                        {
                            Console.WriteLine("请输入新的文件名：");
                            String filenames = Console.ReadLine();
                            renameFile(newpath, filename, filenames);
                        }
                        else
                        {
                            Console.WriteLine("操作完成!");
                        }
                    }
                }
                else
                    Console.WriteLine("文件不存在！");
            }
        }

        /// <summary>
        /// 重命名文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="oldFileName"></param>
        /// <param name="newFileName"></param>
        public static void renameFile(String path, String oldFileName, String newFileName)
        {
            if (!oldFileName.Equals(newFileName))
            {
                var oldDic = path + "\\" + oldFileName;
                DirectoryInfo oldfile = new DirectoryInfo(@oldDic);
                var newFile = path + "\\" + newFileName;
                DirectoryInfo newfile = new DirectoryInfo(@newFile);
                if (!newfile.Exists)
                {
                    oldfile.MoveTo(path + "\\" + newFileName);
                }
                else
                {
                    Console.WriteLine("文件已存在,是否覆盖？1：是；2：取消修改。");
                    int key = Convert.ToInt32(Console.ReadLine());
                    if (key == 1)
                    {
                        Directory.Delete(@newFile, true);
                        oldfile.MoveTo(path + "\\" + newFileName);
                        Console.WriteLine("操作成功！");
                    }
                    else
                    {
                        Console.WriteLine("已取消修改！");
                    }
                }

            }
            else
            {
                Console.WriteLine("新名称与旧名称一致,是否重新命名？1：是；2：取消修改。");
                int key = Convert.ToInt32(Console.ReadLine());
                if (key == 1)
                {
                    Console.WriteLine("请输入新的文件名：");
                    String newFileNames = Console.ReadLine();
                    renameFile(path, oldFileName, newFileNames);
                }
                else
                {
                    Console.WriteLine("已取消修改！");
                }
            }
        }


        /// <summary>
        /// 获得指定目录及其子目录的所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<FileInfo> GetAllFilesByDir(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            //找到该目录下的文件
            FileInfo[] fi = dir.GetFiles();

            //把FileInfo[]数组转换为List
            List<FileInfo> list = fi.ToList<FileInfo>();

            //找到该目录下的所有目录里的文件
            DirectoryInfo[] subDir = dir.GetDirectories();
            foreach (DirectoryInfo d in subDir)
            {
                List<FileInfo> subList = GetFilesByDir(d.FullName);
                foreach (FileInfo subFile in subList)
                {
                    list.Add(subFile);
                }
            }
            return list;
        }

        /// <summary>
        /// 获得指定目录下的所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesByDir(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            //找到该目录下的文件
            FileInfo[] fi = di.GetFiles();

            //把FileInfo[]数组转换为List
            List<FileInfo> list = fi.ToList<FileInfo>();
            return list;
        }
    }
}
