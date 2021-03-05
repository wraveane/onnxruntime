﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ML.OnnxRuntime
{
    internal class PinnedGCHandle : IDisposable
    {
        private GCHandle _handle;

        public PinnedGCHandle(GCHandle handle)
        {
            _handle = handle;
        }

        public IntPtr Pointer
        {
            get
            {
                return _handle.AddrOfPinnedObject();
            }
        }

        #region Disposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _handle.Free();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // No need for the finalizer
        // If this is not disposed timely GC can't help us
        #endregion
    }

    /// <summary>
    /// This helper class contains methods to create native OrtValue from a managed value object
    /// </summary>
    internal static class NativeOnnxValueHelper
    {
        /// <summary>
        /// Converts C# UTF-16 string to UTF-8 zero terminated
        /// byte[] instance
        /// </summary>
        /// <param name="s">string to be converted</param>
        /// <returns>UTF-8 encoded equivalent</returns>
        internal static byte[] StringToZeroTerminatedUtf8(string s)
        {
            byte[] utf8Bytes = UTF8Encoding.UTF8.GetBytes(s);
            Array.Resize(ref utf8Bytes, utf8Bytes.Length + 1);
            utf8Bytes[utf8Bytes.Length - 1] = 0;
            return utf8Bytes;
        }

        /// <summary>
        /// Reads UTF-8 encode string from a C zero terminated string
        /// and converts it into a C# UTF-16 encoded string
        /// </summary>
        /// <param name="nativeUtf8">pointer to native or pinned memory where Utf-8 resides</param>
        /// <returns></returns>
        internal static string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            // .NET 5.0 has Marshal.PtrToStringUTF8 that does the below
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, len);
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        // Delegate for string extraction from an arbitrary objects
        internal delegate string StringExtractor<in TInput>(TInput input);

        /// <summary>
        /// Extracts strings from a read only collection of objects using the provided extractor 
        /// and converts each to UTF-8 zero terminated byte[] instances 
        /// and returns pinned handles to each of them 
        /// </summary>
        /// <param name="inputs">input convert to zero terminated utf8 and pin</param>
        /// <param name="extractor">delegate to extract strings from the provided objects</param>
        /// <param name="cleanupList">list to add pinned memory to for later disposal</param>
        /// <returns></returns>
        internal static IntPtr[] ConvertToUtf8AndPin<T>(IReadOnlyCollection<T> inputs, StringExtractor<T> extractor,
            DisposableList<IDisposable> cleanupList)
        {
            var result = new IntPtr[inputs.Count];
            for (int i = 0; i < inputs.Count; ++i)
            {
                var name = extractor(inputs.ElementAt(i));
                var utf8Name = StringToZeroTerminatedUtf8(name);
                var pinnedHandle = new PinnedGCHandle(GCHandle.Alloc(utf8Name, GCHandleType.Pinned));
                result[i] = pinnedHandle.Pointer;
                cleanupList.Add(pinnedHandle);
            }
            return result;
        }
    }

    internal static class TensorElementTypeConverter
    {
        public static void GetTypeAndWidth(TensorElementType elemType, out Type type, out int width)
        {
            TensorElementTypeInfo result = TensorBase.GetElementTypeInfo(elemType);
            if (result != null)
            {
                type = result.TensorType;
                width = result.TypeSize;
            }
            else
            {
                type = null;
                width = 0;
            }
        }
    }
}
