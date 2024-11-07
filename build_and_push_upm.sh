#!/bin/bash

# 定义变量
PACKAGE_NAME="UGUIPageContainer"
UPM_BRANCH="upm"
UPM_DIR="upm_package"
REPO_URL="https://github.com/mayqiyue/UGUIPageContainer.git"

# 检查是否传入版本号参数
if [ -z "$1" ]; then
    echo "Error: You must provide a version number as an argument."
    echo "Usage: ./build_and_push_upm.sh <version>"
    exit 1
fi

VERSION=$1

# 检查当前目录是否包含 package.json
if [ ! -f "package.json" ]; then
    echo "Error: package.json not found in the current directory."
    exit 1
fi

# 更新 package.json 中的版本号
echo "Updating package.json version to $VERSION..."
jq --arg version "$VERSION" '.version = $version' package.json > temp.json && mv temp.json package.json

# 清理之前的构建目录
echo "Cleaning previous build directory..."
rm -rf $UPM_DIR

# 创建 UPM 包目录
echo "Creating UPM package directory..."
mkdir $UPM_DIR

# 将需要的文件拷贝到 UPM 包目录
echo "Copying package files..."
cp -r Assets $UPM_DIR/
cp package.json $UPM_DIR/
cp README.md $UPM_DIR/
cp LICENSE $UPM_DIR/

# 检查是否已存在 UPM 分支
if git show-ref --verify --quiet refs/heads/$UPM_BRANCH; then
    echo "UPM branch already exists. Checking it out..."
    git checkout $UPM_BRANCH
else
    echo "Creating new UPM branch..."
    git checkout -b $UPM_BRANCH
fi

# 将 UPM 包文件添加到 Git
echo "Adding UPM package files to Git..."
git rm -rf .
cp -r $UPM_DIR/* .
git add .

# 提交更改
echo "Committing the package..."
git commit -m "Build UPM package for $PACKAGE_NAME version $VERSION"

# 推送到远程仓库
echo "Pushing to Git repository..."
git push -u origin $UPM_BRANCH

# 为该版本打标签
echo "Tagging version $VERSION..."
git tag -a "$VERSION" -m "Version $VERSION"

# 推送标签到远程仓库
echo "Pushing tags to remote repository..."
git push origin "$VERSION"

# 切回主分支
echo "Switching back to the main branch..."
git checkout main

# 完成
echo "UPM package build and push complete for version $VERSION!"