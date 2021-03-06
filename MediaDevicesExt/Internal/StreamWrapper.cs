﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MediaDevices
{
    internal class StreamWrapper : Stream
    {
        private IStream stream;

        private void CheckDisposed()
        {
            if (this.stream == null)
            {
                throw new ObjectDisposedException("StreamWrapper");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.stream != null)
            {
                Marshal.ReleaseComObject(this.stream);
                this.stream = null;
            }
        }

        public StreamWrapper(IStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.stream = stream;
        }

        public StreamWrapper(PortableDeviceApiLib.IStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.stream = (IStream)stream;
        }


        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void Flush()
        {
            this.stream.Commit(0);
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                System.Runtime.InteropServices.ComTypes.STATSTG stat;
                this.stream.Stat(out stat, 1); //STATFLAG_NONAME
                return stat.cbSize;
            }
        }

        public override long Position
        {
            get
            {
                return Seek(0, SeekOrigin.Current);
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] localBuffer = buffer;

            if (offset > 0)
            {
                localBuffer = new byte[count];
            }

            IntPtr bytesReadPtr = Marshal.AllocCoTaskMem(sizeof(int));

            try
            {
                this.stream.Read(localBuffer, count, bytesReadPtr);
                int bytesRead = Marshal.ReadInt32(bytesReadPtr);

                if (offset > 0)
                {
                    Array.Copy(localBuffer, 0, buffer, offset, bytesRead);
                }

                return bytesRead;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            finally
            {
                Marshal.FreeCoTaskMem(bytesReadPtr);
            }
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();

            int dwOrigin;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    dwOrigin = 0;   // STREAM_SEEK_SET
                    break;

                case SeekOrigin.Current:
                    dwOrigin = 1;   // STREAM_SEEK_CUR
                    break;

                case SeekOrigin.End:
                    dwOrigin = 2;   // STREAM_SEEK_END
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            IntPtr posPtr = Marshal.AllocCoTaskMem(sizeof(long));

            try
            {
                this.stream.Seek(offset, dwOrigin, posPtr);
                return Marshal.ReadInt64(posPtr);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            finally
            {
                Marshal.FreeCoTaskMem(posPtr);
            }
            return 0;
        }

        public override void SetLength(long value)
        {
            CheckDisposed();

            stream.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] localBuffer = buffer;

            if (offset > 0)
            {
                localBuffer = new byte[count];
                Array.Copy(buffer, offset, localBuffer, 0, count);
            }

            // workaround for Windows 10 Update 1703 problem 
            // https://social.msdn.microsoft.com/Forums/en-US/7f7a045d-9d9d-4ff4-b8e3-de2d7477a177/windows-10-update-1703-problem-with-wpd-and-mtp?forum=csharpgeneral
            IntPtr pcbWritten = Marshal.AllocHGlobal(16);
            stream.Write(localBuffer, count, pcbWritten);
            Marshal.FreeHGlobal(pcbWritten);
        }
    }
}
