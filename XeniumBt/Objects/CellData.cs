using System;

namespace XeniumBt.Objects {

    [Serializable]
    public class CellData {

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int cell;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public string text;

        // ReSharper disable once MemberCanBePrivate.Global
        public CellData() { }

        public CellData(int cell, string text) {
            this.cell = cell;
            this.text = text;
        }

    }
}