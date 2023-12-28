/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * Copyright (c) 2023 Daniel Molinero Lucas
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 * 
 * FPSMetric.cs
 * You can use this class to check the current FPS of your game.
 */

using System;

namespace ScapeCore.Targets
{
    public sealed class FPSMetric
    {
        private static int _fps;
        private static int _framesSumatory;
        private static TimeSpan _fpsStartTime;
        private static TimeSpan _fpsEndTime;

        public static int FPS { get => _fps; }

        internal FPSMetric()
        {
            if (_fpsStartTime == TimeSpan.Zero)
                _fpsStartTime = DateTime.Now.TimeOfDay;
            _framesSumatory++;
            _fpsEndTime = DateTime.Now.TimeOfDay;
            if (_fpsEndTime.Seconds == _fpsStartTime.Seconds + 1)
            {
                _fpsStartTime = TimeSpan.Zero;
                _fpsEndTime = TimeSpan.Zero;
                _fps = _framesSumatory;
                _framesSumatory = 0;
            }
        }
    }
}
