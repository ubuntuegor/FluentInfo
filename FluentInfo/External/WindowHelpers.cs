// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace ManagedCommon;

public class WindowHelpers
{
    public static void BringToForeground(IntPtr handle)
    {
        var input = new NativeMethods.INPUT { type = NativeMethods.INPUTTYPE.INPUT_MOUSE };
        var inputs = new[] { input };
        _ = NativeMethods.SendInput(1, inputs, NativeMethods.INPUT.Size);

        NativeMethods.SetForegroundWindow(handle);

        var fgHandle = NativeMethods.GetForegroundWindow();

        if (fgHandle != handle)
        {
            var threadId1 = NativeMethods.GetWindowThreadProcessId(handle, IntPtr.Zero);
            var threadId2 = NativeMethods.GetWindowThreadProcessId(fgHandle, IntPtr.Zero);

            if (threadId1 != threadId2)
            {
                NativeMethods.AttachThreadInput(threadId1, threadId2, true);
                NativeMethods.SetForegroundWindow(handle);
                NativeMethods.AttachThreadInput(threadId1, threadId2, false);
            }
            else
            {
                NativeMethods.SetForegroundWindow(handle);
            }
        }
    }
}