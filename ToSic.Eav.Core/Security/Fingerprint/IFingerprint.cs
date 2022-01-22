﻿/*
 * Copyright 2022 by 2sic internet solutions in Switzerland - www.2sic.com
 *
 * This file and the code IS COPYRIGHTED.
 * 1. You may not change it.
 * 2. You may not copy the code to reuse in another way.
 *
 * Copying this or creating a similar service, 
 * especially when used to circumvent licensing features in EAV and 2sxc
 * is a copyright infringement.
 *
 * Please remember that 2sic has sponsored more than 10 years of work,
 * and paid more than 1 Million USD in wages for its development.
 * So asking for support to finance advanced features is not asking for much. 
 *
 */
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Security.Fingerprint
{
    /// <summary>
    /// Defines all classes which can provide fingerprints.
    /// For example a system fingerprint or a service fingerprint
    /// </summary>
    [PrivateApi("internal, no need to publish this")]
    public interface IFingerprint
    {
        string GetFingerprint();
    }
}