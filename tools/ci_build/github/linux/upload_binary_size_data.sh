#!/bin/bash
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
set -x
$BUILD_SOURCESDIRECTORY/tools/ci_build/github/windows/post_binary_sizes_to_dashboard.py \
    --ignore_db_error \
    --commit_hash $1 \
    --size_data_file $2 \
    --build_project $3 \
    --build_id $4
