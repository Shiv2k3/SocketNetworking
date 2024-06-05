using System;
using System.Collections;
using System.Collections.Generic;
using Core.Multiplayer.DataTransmission;
using Core.Util;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestLobbyQuery
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestsLobbyQuerySimplePasses()
    {
        var lobbyName = "Lobby34242";
        LobbyQuery ls = new(lobbyName);
        Assert.IsTrue(ls.Search.Value == lobbyName);

        List<string> strs = new();
        for (int i = 0; i < 0; i++)
        {
            strs.Add("9381");
        }

        int length = StringArray.GetHeaderSize(strs.ToArray());
        byte[] arr = new byte[length + 4]; 
        ArraySegment<byte> body = new(arr, 4, length);
        StringArray sa = new(body, 0, strs.ToArray());
        for (int i = 0; i < sa.Count.Value; i++)
        {
            Assert.IsTrue(sa[i].Value == strs[i]);
        }
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestsLobbyQueryWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
