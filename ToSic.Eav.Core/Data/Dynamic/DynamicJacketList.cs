using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    public class DynamicJacketList : IList<object>
    {
        public JArray OriginalData;

        public DynamicJacketList(JArray originalData)
        {
            OriginalData = originalData;
        }


        /// <summary>
        /// Enable enumeration - for both arrays as well as objects <br/>
        /// When going through objects (properties) it will return the keys, not the values. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<object> GetEnumerator()
        {
            if (OriginalData is JArray jArray)
                return jArray.Select(DynamicJacket.WrapOrUnwrap).GetEnumerator();
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count
        {
            get
            {
                if (OriginalData is JArray jArray)
                    return jArray.Count;
                return 0;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => true;

        /// <summary>
        /// Access the items in this object - but only if the underlying object is an array. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                if (OriginalData is JArray jArray)
                    return DynamicJacket.WrapOrUnwrap(jArray[index]);
                return null;
            }
            set => throw new NotImplementedException();
        }

        #region Not Implemented Stuff of IList

        [PrivateApi]
        public void Add(object item) => throw new NotImplementedException();

        [PrivateApi]
        public void Clear() => throw new NotImplementedException();

        [PrivateApi]
        public bool Contains(object item) => throw new NotImplementedException();

        [PrivateApi]
        public void CopyTo(object[] array, int arrayIndex) => throw new NotImplementedException();

        [PrivateApi]
        public bool Remove(object item) => throw new NotImplementedException();

        [PrivateApi]
        public int IndexOf(object item) => throw new NotImplementedException();

        [PrivateApi]
        public void Insert(int index, object item) => throw new NotImplementedException();

        [PrivateApi]
        public void RemoveAt(int index) => throw new NotImplementedException();


        #endregion
    }
}
