﻿
namespace CapnProto_Msg
{
    public static class C_cLoginMsg{
      public const int cLoginMsg = 1;
    }
    [global::CapnProto.StructAttribute(global::CapnProto.ElementSize.InlineComposite, 1, 3)]
    [global::CapnProto.IdAttribute(0x810cdb84e47b9605)]
    public partial struct LoginMsg : global::CapnProto.IPointer
    {
        private global::CapnProto.Pointer ѧ_;
        private LoginMsg(global::CapnProto.Pointer pointer){ this.ѧ_ = pointer; }
        public static explicit operator global::CapnProto_Msg.LoginMsg(global::CapnProto.Pointer pointer) { return new global::CapnProto_Msg.LoginMsg(pointer); }
        public static implicit operator global::CapnProto.Pointer (global::CapnProto_Msg.LoginMsg obj) { return obj.ѧ_; }
        public override int GetHashCode() { return this.ѧ_.GetHashCode(); }
        partial void OnToString(ref string s);
        public override string ToString() { string s = null; this.OnToString(ref s); return s ?? this.ѧ_.ToString(); }
        public override bool Equals(object obj) { return obj is global::CapnProto_Msg.LoginMsg && (this.ѧ_ == ((global::CapnProto_Msg.LoginMsg)obj).ѧ_); }
        global::CapnProto.Pointer global::CapnProto.IPointer.Pointer { get { return this.ѧ_; } }
        public global::CapnProto_Msg.LoginMsg Dereference() { return (global::CapnProto_Msg.LoginMsg)this.ѧ_.Dereference(); }
        public static global::CapnProto_Msg.LoginMsg Create(global::CapnProto.Pointer parent) { return (global::CapnProto_Msg.LoginMsg)parent.Allocate(1, 3); }
        [global::CapnProto.FieldAttribute(0, pointer: 0)]
        [global::System.ComponentModel.DefaultValueAttribute(@"zengyi")]
        public global::CapnProto.Text userName
        {
            get
            {
                return (global::CapnProto.Text)this.ѧ_.GetPointer(0);
            }
            set
            {
                this.ѧ_.SetPointer(0, value);
            }
        }
        [global::CapnProto.FieldAttribute(1, pointer: 1)]
        [global::System.ComponentModel.DefaultValueAttribute(@"111")]
        public global::CapnProto.Text passWord
        {
            get
            {
                return (global::CapnProto.Text)this.ѧ_.GetPointer(1);
            }
            set
            {
                this.ѧ_.SetPointer(1, value);
            }
        }
        [global::CapnProto.FieldAttribute(2, 0, 32)]
        [global::System.ComponentModel.DefaultValueAttribute(0)]
        public int userID
        {
            get
            {
                return (int)(this.ѧ_.GetInt32(0));
            }
            set
            {
                this.ѧ_.SetInt32(0, (int)(value));
            }
        }
        [global::CapnProto.FieldAttribute(3, pointer: 2)]
        public global::CapnProto.FixedSizeList<global::CapnProto.Text> roleList
        {
            get
            {
                return (global::CapnProto.FixedSizeList<global::CapnProto.Text>)this.ѧ_.GetPointer(2);
            }
            set
            {
                this.ѧ_.SetPointer(2, value);
            }
        }
    }
}