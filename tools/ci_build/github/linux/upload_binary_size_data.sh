#!/bin/bash
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
set -e
set -x

python3 $BUILD_SOURCESDIRECTORY/tools/ci_build/github/windows/post_binary_sizes_to_dashboard.py \
    --ignore_db_error \
    --commit_hash $BUILD_SOURCEVERSION \
    --size_data_file $1 \
    --build_project $2 \
    --build_id $BUILD_ID
