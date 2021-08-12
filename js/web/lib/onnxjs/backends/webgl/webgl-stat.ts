// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import {env} from 'onnxruntime-common';

interface Stat {
  countTextureCreated: number;
  countTextureDeleted: number;

  countArtifactBuilt: number;

  countTexImage2D: number;
  countTexSubImage2D: number;
  countReadPixels: number;
}

// eslint-disable-next-line @typescript-eslint/naming-convention
const wasm: {STAT: Stat} = env.wasm as unknown as {STAT: Stat};

if (!wasm.STAT) {
  wasm.STAT = {
    countTextureCreated: 0,
    countTextureDeleted: 0,

    countArtifactBuilt: 0,

    countTexImage2D: 0,
    countTexSubImage2D: 0,
    countReadPixels: 0
  };
}

export const STAT = wasm.STAT;
