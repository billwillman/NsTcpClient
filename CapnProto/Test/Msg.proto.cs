using Capnp;
using Capnp.Rpc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [TypeId(0x810cdb84e47b9605UL)]
    public class LoginMsg : ICapnpSerializable
    {
        public const UInt64 typeId = 0x810cdb84e47b9605UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            UserName = reader.UserName;
            PassWord = reader.PassWord;
            UserID = reader.UserID;
            RoleList = reader.RoleList;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.UserName = UserName;
            writer.PassWord = PassWord;
            writer.UserID = UserID;
            writer.RoleList.Init(RoleList);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
            UserName = UserName ?? "zengyi";
            PassWord = PassWord ?? "111";
        }

        public string UserName
        {
            get;
            set;
        }

        public string PassWord
        {
            get;
            set;
        }

        public int UserID
        {
            get;
            set;
        }

        = 0;
        public IReadOnlyList<string> RoleList
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public string UserName => ctx.ReadText(0, "zengyi");
            public string PassWord => ctx.ReadText(1, "111");
            public int UserID => ctx.ReadDataInt(0UL, 0);
            public IReadOnlyList<string> RoleList => ctx.ReadList(2).CastText2();
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public string UserName
            {
                get => this.ReadText(0, "zengyi");
                set => this.WriteText(0, value, "zengyi");
            }

            public string PassWord
            {
                get => this.ReadText(1, "111");
                set => this.WriteText(1, value, "111");
            }

            public int UserID
            {
                get => this.ReadDataInt(0UL, 0);
                set => this.WriteData(0UL, value, 0);
            }

            public ListOfTextSerializer RoleList
            {
                get => BuildPointer<ListOfTextSerializer>(2);
                set => Link(2, value);
            }
        }
    }
}