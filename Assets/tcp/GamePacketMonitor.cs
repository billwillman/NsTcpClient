/*
 * 客户端发包频率检查器
 */

#if DEBUG
    #define _GAMEPACKET_MONITOR
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
//using LitJson;
//using NsLib.ResMgr;

public class GamePacketMonitor {

    public const long WATCHER_ACTION_DEFAULT = 0;
    public const long WATCHER_ACTION_KICK = 1;
    public const long WATCHER_ACTION_DISCARD = 2;

    public class WatcherBase {
        //操作码
        public int op {
            get;
            set;
        }
        //计数周期，毫秒
        public int itvl {
            get;
            set;
        }
        //周期内次数
        public int times {
            get;
            set;
        }
        //周期内超界的行为
        public int act {
            get;
            set;
        }
    }

    public class PacketCounter {
        public uint op {
            get;
            set;
        }
        public ulong endStamp {
            get;
            set;
        }

        public uint times {
            get;
            set;
        }

        public PacketCounter(uint op) {
            this.op = op;
            endStamp = 0;
            times = 0;
        }
    }

    public class PacketMonitor {
        private Dictionary<uint, PacketCounter> m_CounterMap = new Dictionary<uint, PacketCounter>();
        private ulong m_StartTickCount;

        public PacketMonitor() {
            m_StartTickCount = (ulong)System.Environment.TickCount;
        }

        private long CheckAction(uint op, ulong now) {
#if _GAMEPACKET_MONITOR
            var watcher = WatcherBaseManager.GetInstance().GetWatcherBase(op);
            if (watcher == null)
                watcher = WatcherBaseManager.GetInstance().GetWatcherBase(0);
            if (watcher == null)
                return WATCHER_ACTION_DEFAULT;
            if (watcher.itvl == 0 || watcher.times == 0)
                return WATCHER_ACTION_DEFAULT;
            PacketCounter counter;
            if (!m_CounterMap.TryGetValue(op, out counter) || counter == null) {
                // 初始化
                counter = new PacketCounter(op);
                //----
                m_CounterMap[op] = counter;
            }
            if (now >= counter.endStamp) {
                counter.endStamp = now + (ulong)watcher.itvl;
                counter.times = 0;
            }

            // 产生轮寻了
            if (m_StartTickCount > now) {
                m_StartTickCount = now;
                // 产生轮寻的时候，就重新置一下
                counter.endStamp = now + (ulong)watcher.itvl;
                counter.times = 0;
            }

            counter.times++;
            if (counter.times > watcher.times)
                return watcher.act;
            return WATCHER_ACTION_DEFAULT;
#else
            return WATCHER_ACTION_DEFAULT;
#endif

        }

        // 打印堆栈信息
        static public void DebugStackTrackInfo() {
            string info = string.Empty;
            StackTrace st = new StackTrace(true);
            //得到当前的所以堆栈  
            StackFrame[] sf = st.GetFrames();
            if (sf != null) {
                for (int i = 0; i < sf.Length; ++i) {
                    info = info + "\r\n" + " FileName=" + sf[i].GetFileName() + " fullname=" + sf[i].GetMethod().DeclaringType.FullName + " function=" + sf[i].GetMethod().Name + " FileLineNumber=" + sf[i].GetFileLineNumber();
                }
                if (!string.IsNullOrEmpty(info)) {

#if DEBUG
                    UnityEngine.Debug.LogError(info);
#endif

                }
            }
        }

        // 子线程调用
        public bool CheckAction(uint op) {
            bool ret = false;
#if _GAMEPACKET_MONITOR
            //System.DateTime.Now.Ticks;
            ulong now = (ulong)System.Environment.TickCount;
            long rule = CheckAction(op, now);
            switch (rule) {
                case WATCHER_ACTION_KICK:
                    ret = true;
#if DEBUG
                    UnityEngine.Debug.LogErrorFormat("【Discard】Protocal check hits the policy, opcode = {0}", op.ToString());
                    DebugStackTrackInfo();
#endif
                    break;
                case WATCHER_ACTION_DISCARD:
                    ret = true;
#if DEBUG
                    UnityEngine.Debug.LogErrorFormat("【Kick】Protocal check hits the policy, opcode = {0}", op.ToString());
                    DebugStackTrackInfo();
#endif
                    break;
            }
#endif
            return ret;
        }
    }

    public class WatcherBaseManager : Singleton<WatcherBaseManager> {
        private Dictionary<string, WatcherBase> m_WatcherMap = null;
        private void Load(string fileName) {
            /*
#if _GAMEPACKET_MONITOR
            string str = ResourceMgr.Instance.LoadTextConfigByPath(fileName);
            try {
                m_WatcherMap = LitJson.JsonMapper.ToObject<Dictionary<string, WatcherBase>>(str);
            } catch (System.Exception e) {
#if DEBUG
                UnityEngine.Debug.LogError(e.ToString());
#endif
            }
#endif
*/
        }

        public void Load() {
            Load("Config/Net/PacketWatcher.json");
        }

        public WatcherBase GetWatcherBase(uint op) {
            WatcherBase ret;
            if (m_WatcherMap == null || !m_WatcherMap.TryGetValue(op.ToString(), out ret) || ret == null)
                return null;
            return ret;
        }
    }
}

