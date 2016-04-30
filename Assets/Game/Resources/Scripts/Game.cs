﻿using SLua;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    private static LuaSvr _l;
    private int _progress = 0;

    public UnityAction<int> onProgressHandler;

    void Start()
    {
        if(_l == null)
        {
#if UNITY_5
        Application.logMessageReceived += this.log;
#else
		Application.RegisterLogCallback(this.log);
#endif
            LuaState.loaderDelegate = _loadFileWithSuffix;
            _l = new LuaSvr();
            _l.init(tick, complete, LuaSvrFlag.LSF_BASIC);
        }
        else
        {
            complete();
        }
    }

    public static LuaSvr GetLuaSvr()
    {
        return _l;
    }

    void log(string cond, string trace, LogType lt)
    {
        Debug.Log(cond);
    }

    void tick(int p)
    {
        _progress = p;

        if (onProgressHandler != null)
        {
            onProgressHandler.Invoke(p);
        }
    }

    protected byte[] _loadFileWithSuffix(string strFile)
    {
        if (string.IsNullOrEmpty(strFile))
        {
            return null;
        }

        strFile.Replace(".", "/");
        strFile += LGameConfig.FILE_AFFIX_LUA;

        string strLuaPath = LGameConfig.DATA_CATAGORY_LUA + Path.DirectorySeparatorChar + strFile;
        string strFullPath = LGameConfig.GetInstance().GetLoadUrl(strLuaPath);

        // Read from file.
        LArchiveBinFile cArc = new LArchiveBinFile();
        if (!cArc.Open(strFullPath, FileMode.Open, FileAccess.Read))
        {
            return null;
        }

        if (!cArc.IsValid())
        {
            return null;
        }

        int nContentLength = (int)cArc.GetStream().Length;
        byte[] aContents = new byte[nContentLength];
        cArc.ReadBuffer(ref aContents, nContentLength);
        cArc.Close();

        return aContents;
    }

    void complete()
    {
        _l.start("main");
    }
}
