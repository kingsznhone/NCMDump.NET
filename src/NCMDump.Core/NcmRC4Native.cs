using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace NCMDump.Core
{
    public sealed partial class NcmRC4Native : IDisposable
    {
        private const string DllName = "Resources/RC4Transformer";
        private RC4TransformerSafeHandle _transformerHandle;
        private bool disposedValue;

        [LibraryImport(DllName, EntryPoint = "CreateTransformContext")]
        private static partial nint CreateTransformContext([In] byte[] config, nuint config_len);

        [LibraryImport(DllName, EntryPoint = "DestroyTransformContext")]
        private static partial void DestroyTransformContext(nint ctx);

        [LibraryImport(DllName, EntryPoint = "TransformData")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static partial bool TransformData(SafeHandle ctx, Span<byte> data, nuint dataLength);

        public NcmRC4Native(byte[] key)
        {
            if (key == null || key.Length == 0) throw new ArgumentException("key must not be null or empty", nameof(key));
            nint ctx = CreateTransformContext(key, (nuint)key.Length);
            if (ctx == nint.Zero) throw new InvalidOperationException("Failed to create native RC4 context");
            _transformerHandle = new RC4TransformerSafeHandle(ctx);
        }

        public byte[] Transform(byte[] data)
        {
            Transform(data.AsSpan());
            return data;
        }

        public int Transform(Memory<byte> data)
        {
            return Transform(data.Span);
        }

        public unsafe int Transform(Span<byte> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (_transformerHandle == null || _transformerHandle.IsInvalid)
            {
                throw new ObjectDisposedException(nameof(NcmRC4Native));
            }

            bool result = TransformData(_transformerHandle, data, (nuint)data.Length);
            if (!result) throw new InvalidOperationException("Native transform failed");

            return data.Length;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _transformerHandle?.Dispose();
                    _transformerHandle = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private sealed class RC4TransformerSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public RC4TransformerSafeHandle() : base(true)
            {
            }

            public RC4TransformerSafeHandle(IntPtr handle) : base(true)
            {
                SetHandle(handle);
            }

            protected override bool ReleaseHandle()
            {
                if (!IsInvalid)
                {
                    DestroyTransformContext(handle);
                }
                return true;
            }
        }
    }
}