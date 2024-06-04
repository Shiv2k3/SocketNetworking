using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TString
{
    // A Test behaves as an ordinary method
    [Test]
    public void TStringSimplePasses()
    {
        byte[] stream = new byte[300909];
        int hs = 45;
        ArraySegment<byte> body = new(stream, hs, stream.Length - hs);
        TString a = new("shiva", body, hs);
        TString a1 = new(a.Stream, hs);
        Assert.IsTrue(a.Value == a1.Value);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TStringWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
