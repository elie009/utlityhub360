# How to Remove Secrets from Git History

## Step 1: Remove appsettings files from git tracking (but keep local files)

Run these commands in your repository root:

```bash
# Remove from git tracking but keep local files
git rm --cached UtilityHub360/appsettings.json
git rm --cached UtilityHub360/appsettings.Development.json
git rm --cached UtilityHub360/appsettings.Production.json
git rm --cached UtilityHub360/appsettings.Staging.json

# Remove all publish folders
git rm -r --cached publish/ 2>$null
git rm -r --cached UtilityHub360/publish/ 2>$null
```

## Step 2: Commit the removal

```bash
git add .gitignore
git commit -m "Remove appsettings files with secrets from git tracking"
```

## Step 3: Clean git history (if secrets are in commit history)

**Option A: Use git filter-branch (for small repos)**
```bash
git filter-branch --force --index-filter \
  "git rm --cached --ignore-unmatch UtilityHub360/appsettings*.json" \
  --prune-empty --tag-name-filter cat -- --all
```

**Option B: Use BFG Repo-Cleaner (Recommended - faster)**
1. Download BFG: https://rtyley.github.io/bfg-repo-cleaner/
2. Create a file `secrets.txt` with the API key:
   ```
   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
   ```
3. Run:
   ```bash
   java -jar bfg.jar --replace-text secrets.txt
   git reflog expire --expire=now --all
   git gc --prune=now --aggressive
   ```

## Step 4: Force push (⚠️ WARNING: This rewrites history)

```bash
git push origin myfix --force
```

**⚠️ IMPORTANT:** 
- This will rewrite git history
- All team members will need to re-clone or reset their local repos
- Make sure everyone is aware before force pushing

## Alternative: Create template files

Instead of removing, you can create template files:

1. Create `appsettings.json.template` with placeholder:
   ```json
   {
     "OpenAISettings": {
       "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
     }
   }
   ```

2. Keep real `appsettings.json` in `.gitignore`
3. Document that developers should copy the template and add their key

## Quick Fix (If you just want to stop tracking now)

```bash
# Remove from git but keep files locally
git rm --cached UtilityHub360/appsettings*.json

# Commit
git commit -m "Stop tracking appsettings files with secrets"

# Push (this will still have secrets in history, but new commits won't)
git push origin myfix
```

