﻿// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using FIVES;
using ScalabilityPlugin;
using ServerSyncPlugin;
using System;
using System.Runtime.Serialization;

namespace ConfigScalabilityPlugin
{
    [Serializable]
    class ConfigDoR : IDomainOfResponsibility
    {
        public ConfigDoR()
        {
        }

        public bool IsResponsibleFor(Entity entity)
        {
            var positionValue = entity["location"]["position"].Value;
            Vector position = (Vector)positionValue;
            return position.x >= MinX && position.x < MaxX && position.y >= MinY && position.y < MaxY;
        }

        public override string ToString()
        {
            return String.Format("minX = {0}, maxX = {1}, minY = {2}, maxY = {3}", MinX, MaxX, MinY, MaxY);
        }

        public event EventHandler Changed;

        #region ISerializable interface

        public ConfigDoR(SerializationInfo info, StreamingContext context)
        {
            minX = info.GetDouble("minX");
            maxX = info.GetDouble("maxX");
            minY = info.GetDouble("minY");
            maxY = info.GetDouble("maxY");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("minX", minX);
            info.AddValue("maxX", maxX);
            info.AddValue("minY", minY);
            info.AddValue("maxY", maxY);
        }

        #endregion

        internal double MinX
        {
            get
            {
                return minX;
            }
            set
            {
                minX = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        internal double MaxX
        {
            get
            {
                return maxX;
            }
            set
            {
                maxX = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        internal double MinY
        {
            get
            {
                return minY;
            }
            set
            {
                minY = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        internal double MaxY
        {
            get
            {
                return maxY;
            }
            set
            {
                maxY = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;
    }
}
