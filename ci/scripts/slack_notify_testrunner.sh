#!/bin/bash

: ${COMMIT_RANGE:="$CIRCLE_SHA1^...$CIRCLE_SHA1"}

: ${SLACK_WEBHOOK?}
: ${SLACK_FOOTER:=}
: ${SLACK_AUTHOR_NAME:=}
: ${SLACK_AUTHOR_ICON:=}
: ${SLACK_AUTHOR_LINK:=}
: ${SLACK_THUMBNAIL:="https://img.icons8.com/ultraviolet/100/000000/sedan.png"}

: ${TEST_PASSED:=0}
: ${TEST_FAILED:=0}
: ${TEST_SKIPPED:=0}
: ${TEST_INCONCLUSIVE:=0}
TEST_TOTAL=$((TEST_PASSED+TEST_FAILED+TEST_SKIPPED+TEST_INCONCLUSIVE))

: ${TEST_ERRORS:=}
: ${BUILD_STATUS:="fail"}

: ${PLAYGROUND_CARCONTROLLER_VERSION:=}
: ${PLAYGROUND_UI_VERSION:=}
: ${MELLIS_VERSION:=}
: ${MELLIS_PYTHON3_VERSION:=}
: ${UNITY_VERSION:=}

if [ -z "$SLACK_WEBHOOK" ]; then
    echo "NO SLACK WEBHOOK SET"
    echo "Please input your SLACK_WEBHOOK value either in the settings for this project, or as a parameter for this orb."
    exit 1
fi

function quoteNotFirst {
    local quoteSymbol=""
    while read -r line
    do
        printf "$quoteSymbol%s\n" "$line"
        quoteSymbol="> "
    done <<< "$1"
}

function quote {
    while read -r line
    do
        printf "> %s\n" "$line"
    done <<< "$1"
}

function escapeJson {
    : ${1?}
    local val=${1//\\/\\\\} # \ 
    # val=${val//\//\\\/} # / 
    # val=${val//\'/\\\'} # ' (not strictly needed ?)
    val=${val//\"/\\\"} # " 
    val=${val//	/\\t} # \t (tab)
    # val=${val//^M/\\\r} # \r (carriage return)
    val="$(echo "$val" | tr -d '\r')" # \r (carrige return)
    val=${val//
/\\\n} # \n (newline)
    val=${val//^L/\\\f} # \f (form feed)
    val=${val//^H/\\\b} # \b (backspace)
    echo -n "$val"
}

function getTextForCommit {
    local commitSHA=${1?}
    # local commitSHA="$(git show --pretty=%H --quiet ${1?})"
    local commitShortSHA="$(git show --pretty=%h --quiet $commitSHA)"
    local commitMessage="$(quoteNotFirst "$(git show --quiet --pretty=%B $commitShortSHA)")"

    echo "> <https://github.com/$CIRCLE_PROJECT_USERNAME/$CIRCLE_PROJECT_REPONAME/commit/$commitSHA|$commitShortSHA> $(escapeJson "$commitMessage")"
}

echo "BUILD_STATUS=$BUILD_STATUS"

if [ "$BUILD_STATUS" == "success" ]
then
    echo "Build successful, adjusing message accordingly"
    # Success
    color="#1CBF43" # green
    title=":tada: TESTING COMPLETED SUCCESSFULLY"
    fallback="Build completed successfully ($CIRCLE_JOB#$CIRCLE_BUILD_NUM)"
    visitJobActionStyle="primary"
else
    echo "Build failed, adjusing message accordingly"
    # Fail
    color="#ed5c5c" # red
    title=":no_entry_sign: TESTING FAILED"
    fallback="Build failed ($CIRCLE_JOB#$CIRCLE_BUILD_NUM)"
    visitJobActionStyle="danger"

    if [ "$TEST_FAILED" -gt 0 ] && [ "$TEST_ERRORS" ]
    then
        errorsField="*Errors:*\\n\\n$(escapeJson "$TEST_ERRORS")"
    fi
fi
if [[ "$TEST_FAILED" == "0" ]]; then
    testFailed="Failed: $TEST_FAILED"
else
    testFailed="Failed: $TEST_FAILED :exclamation:"
fi
if [[ "$TEST_INCONCLUSIVE" == "0" ]]; then
    testInconclusive="Inconclusive: $TEST_INCONCLUSIVE"
else
    testInconclusive="Inconclusive: $TEST_INCONCLUSIVE :exclamation:"
fi
if [[ "$TEST_SKIPPED" == "0" ]]; then
    testSkipped="Skipped: $TEST_SKIPPED"
else
    testSkipped="Skipped: $TEST_SKIPPED :exclamation:"
fi
if [[ "$TEST_PASSED" == "$TEST_TOTAL" ]] && [[ $TEST_PASSED > 0 ]]; then
    testPassed="Passed: $TEST_PASSED :heavy_check_mark:"
else
    testPassed="Passed: $TEST_PASSED"
fi
testResults="$testPassed\\n$testFailed\\n$testInconclusive\\n$testSkipped"
echo

: ${errorsField:=}

author=""
if [[ "$SLACK_AUTHOR_NAME" ]]
then
    echo "Got custom author from environment variable: '$SLACK_AUTHOR_NAME'"
    author="
        \"author_name\": \"$SLACK_AUTHOR_NAME\",
        \"author_icon\": \"$SLACK_AUTHOR_ICON\",
        \"author_link\": \"$SLACK_AUTHOR_LINK\",
    "
elif [[ "${CIRCLE_USERNAME:-}" ]]
then
    echo "Looking up commit author $CIRCLE_USERNAME on github..."

    curlResult="$(curl -s https://api.github.com/users/$CIRCLE_USERNAME \
    | grep "\"avatar_url\":")"
    curlRegex='avatar_url.*"(.+)"'

    if [[ $curlResult =~ $curlRegex ]] && [[ "${BASH_REMATCH[1]:-}" ]]
    then
        authorIcon=${BASH_REMATCH[1]}
        echo "Found author profile picture: $authorIcon"

        author="
        \"author_name\": \"pushed by $CIRCLE_USERNAME\",
        \"author_icon\": \"$authorIcon\",
        \"author_link\": \"https://github.com/$CIRCLE_USERNAME\",
        "
    else
        echo "No profile picture found."
        author="
        \"author_name\": \"pushed by $CIRCLE_USERNAME\",
        \"author_link\": \"https://github.com/$CIRCLE_USERNAME\",
        "
    fi
fi

function formatVersion {
    if [[ "${1}" ]]; then
        echo "*$(escapeJson "$1")*"
    else
        echo "_(unknown version)_"
    fi
}

if [[ $TEST_TOTAL > 0 ]]; then
    testPercent=$((100*TEST_PASSED/TEST_TOTAL))
else
    testPercent=$((0))
fi

versions="\
Playground car-controller: $(formatVersion "$PLAYGROUND_CARCONTROLLER_VERSION")\\n\
Playground UI: $(formatVersion "$PLAYGROUND_UI_VERSION")\\n\
Mellis: $(formatVersion "$MELLIS_VERSION")\\n\
Python3 module: $(formatVersion "$MELLIS_PYTHON3_VERSION")\\n\
Unity: $(formatVersion "$UNITY_VERSION")"

fields="
{
    \"title\": \"Test results: $testPercent %\",
    \"value\": \"$testResults\",
    \"short\": true
},
{
    \"title\": \"Versions\",
    \"value\": \"$versions\",
    \"short\": true
}"

echo
text=""
commitCount=$((0))
while read commit
do
    echo "Collecting commit: $commit"
    text="$(getTextForCommit $commit)\\n$text"
    ((commitCount++))
done < <(git log --pretty=%h $COMMIT_RANGE)

if [[ $commitCount -eq 1 ]]
then
    text="Commit:\\n$text"
else
    text="$commitCount commits _(oldest first):_\\n$text"
fi

text="*Project: \`$CIRCLE_PROJECT_USERNAME/$CIRCLE_PROJECT_REPONAME\` Branch: \`$CIRCLE_BRANCH\`*\\n$text"

if [[ "$SLACK_FOOTER" ]]
then
    echo "Got custom footer from environment variable: '$SLACK_FOOTER'"
    footer="$SLACK_FOOTER"
else
    footer="$(git diff --shortstat $COMMIT_RANGE)"
    echo "Using git diff as footer: '$footer'"
fi

data=" {
\"attachments\": [
    {
        $author
        \"fallback\": \"$fallback\",
        \"title\": \"$title\",
        \"footer\": \"$footer\",
        \"text\": \"$text\\n$errorsField\",
        \"mrkdwn_in\": [\"fields\", \"text\"], 
        \"color\": \"$color\",
        \"thumb_url\": \"$SLACK_THUMBNAIL\",
        \"fields\": [
            $fields
        ],
        \"actions\": [
            {
                \"style\": \"$visitJobActionStyle\",
                \"type\": \"button\",
                \"text\": \"Visit Job #$CIRCLE_BUILD_NUM ($CIRCLE_STAGE)\",
                \"url\": \"$CIRCLE_BUILD_URL\"
            }
        ]
    }
] }"

echo
response="$(curl -X POST -H 'Content-type: application/json' --data "$data" $SLACK_WEBHOOK)"
if [[ "$response" == "ok" ]]
then
    echo "Job completed successfully. Alert sent."
else
    echo "Something went wrong in the webhook..."
    echo "Response: $response"
fi

echo
echo "Payload:"
echo
echo "$data"
