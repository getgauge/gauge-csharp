set -e
GAUGE_LATEST_NIGHTLY=`curl -w "%{url_effective}\n" -L -s -S https://bintray.com/gauge/Gauge/Nightly/_latestVersion -o /dev/null`

GAUGE_LATEST_NIGHTLY_VERSION=`echo $GAUGE_LATEST_NIGHTLY | sed 's/.*\///'`

BIT=`uname -m`

unamestr=`uname`
if [[ "$unamestr" == 'Linux' ]]; then
	GAUGE_FILE_NAME="gauge-$GAUGE_LATEST_NIGHTLY_VERSION-linux.$BIT.zip"
	GAUGE_DOWNLOAD_URL="https://bintray.com/gauge/Gauge/download_file?file_path=linux%2F$GAUGE_FILE_NAME"
	wget $GAUGE_DOWNLOAD_URL -O $GAUGE_FILE_NAME
	OUTPUT_DIR="./gauge_$GAUGE_LATEST_NIGHTLY_VERSION"
	unzip $GAUGE_FILE_NAME -d $OUTPUT_DIR
	cd $OUTPUT_DIR
	GAUGE_PREFIX=/usr/local GAUGE_PLUGINS=html-report ./install.sh
elif [[ "$unamestr" == 'Darwin' ]]; then
	GAUGE_FILE_NAME="gauge-$GAUGE_LATEST_NIGHTLY_VERSION-darwin.$BIT.zip"
	GAUGE_DOWNLOAD_URL="https://bintray.com/gauge/Gauge/download_file?file_path=darwin%2F$GAUGE_FILE_NAME"
	wget $GAUGE_DOWNLOAD_URL -O $GAUGE_FILE_NAME
	OUTPUT_DIR="./gauge_$GAUGE_LATEST_NIGHTLY_VERSION"
	unzip $GAUGE_FILE_NAME -d $OUTPUT_DIR
	cd $OUTPUT_DIR
	GAUGE_PREFIX=/usr/local GAUGE_PLUGINS=html-report ./install.sh
fi

gauge -v
